# HayatiArac API — Deployment Runbook (FAZ 4)

Bu dosya, API'yi Docker kullanmadan, çıplak bir Ubuntu sunucuda (systemd + Nginx + UFW/Fail2Ban ile) yayınlamanın adım adım runbook'udur. İki aşama var: **Aşama A** (WSL2 üzerinde risksiz prova) ve **Aşama B** (gerçek, internete açık VPS). Adımların iskeleti birebir aynıdır; farklar ilgili başlıklarda belirtildi.

> Komutların neden böyle çalıştığına dair derinlemesine açıklamalar (örn. `/etc` dizin yapısı, systemd kavramları, Nginx reverse proxy mantığı, UFW/Fail2Ban ayrıntıları) için: `D:\Infos\HayatiAracInfos\Deployment\` altındaki notlar.

Destek servisler (SQL Server, Redis, RabbitMQ) bu planın kapsamı dışında — hâlâ `docker-compose.yml` ile Docker'da çalışır. Sadece **API'nin kendisi** Docker'sız, doğrudan `dotnet` + systemd ile çalıştırılıyor.

---

## Aşama A — WSL2 Ubuntu provası

### 0) Ön koşul: WSL2 Ubuntu-22.04 ve .NET runtime

```bash
# Windows PowerShell:
wsl --install -d Ubuntu-22.04

# WSL Ubuntu içinde:
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
dotnet --info   # Microsoft.AspNetCore.App 9.0.x görünmeli
```

### 1) Destek servisleri başlatma (Windows tarafında, PowerShell)

```powershell
cd D:\GitHub\PROJECTS\HayatiArac\HayatiArac
docker compose up -d sqlserver redis rabbitmq
docker compose ps   # üçü de healthy olmalı
```

### 2) `EnvironmentFile` hazırlama (WSL içinde, bir kere)

API'nin gizli bağlantı bilgileri `appsettings.json`'a yazılmaz; systemd'nin `EnvironmentFile`'ı ile dışarıdan, **git'e hiç girmeyen** bir dosyadan okunur.

```bash
sudo mkdir -p /etc/hayatiarac
sudo cp deploy/hayatiarac-api.env.example /etc/hayatiarac/hayatiarac-api.env   # /mnt/d/... üzerinden erişerek
sudo nano /etc/hayatiarac/hayatiarac-api.env   # gerçek şifreleri girin (docker-compose.yml'deki değerlerle aynı)
sudo chmod 600 /etc/hayatiarac/hayatiarac-api.env
```

### 3) Publish + kopyalama + systemd servisi kurma

```bash
# Repo kökünden (WSL/git-bash), tek komutla publish + kopyalama + restart:
./deploy/deploy.sh
```

İlk seferde servis henüz tanımlı olmadığı için, `deploy.sh`'tan önce bir kerelik systemd unit kurulumu gerekir:

```bash
sudo cp deploy/hayatiarac-api.service /etc/systemd/system/hayatiarac-api.service
sudo systemctl daemon-reload
sudo systemctl enable --now hayatiarac-api
sudo systemctl status hayatiarac-api
```

(Bundan sonraki her deploy'da sadece `./deploy/deploy.sh` yeterli — publish alır, `/opt/hayatiarac`'a kopyalar, servisi restart eder.)

### 4) Nginx reverse proxy

```bash
sudo apt-get update
sudo apt-get install -y nginx
sudo cp deploy/nginx-hayatiarac.conf /etc/nginx/sites-available/hayatiarac
sudo ln -s /etc/nginx/sites-available/hayatiarac /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl enable --now nginx
```

**Not:** Config dosyasını sonradan değiştirirseniz Nginx bunu otomatik algılamaz — `sudo systemctl reload nginx` ile açıkça söylemeniz gerekir.

### 5) Doğrulama

```bash
curl http://localhost/health                        # → Healthy
sudo systemctl is-enabled hayatiarac-api             # → enabled

# Crash/restart testi:
sudo systemctl status hayatiarac-api                 # Main PID'i not edin
sudo kill -9 <PID>
sleep 6
sudo systemctl status hayatiarac-api                 # farklı PID, active (running)

# WSL reboot sonrası otomatik başlama testi:
# (Windows PowerShell'de) wsl --shutdown, sonra wsl -d Ubuntu-22.04
sudo systemctl status hayatiarac-api                 # otomatik active (running) olmalı
```

### 6) UFW + Fail2Ban

```bash
sudo apt-get install -y ufw
sudo ufw allow 22/tcp
sudo ufw allow 80,443/tcp
sudo ufw enable
sudo ufw status verbose

sudo apt-get install -y fail2ban
sudo tee /etc/fail2ban/jail.local > /dev/null << 'EOF'
[sshd]
enabled = true
port = 22
filter = sshd
logpath = /var/log/auth.log
maxretry = 5
bantime = 600
findtime = 600
EOF
sudo systemctl restart fail2ban
sudo fail2ban-client status sshd
```

Let's Encrypt/SSL bu aşamada **atlanır** (gerçek domain gerektirir) — Aşama B'de yapılır.

---

## Aşama B — Gerçek VPS + domain

Aynı iskelet, farklar:

1. **SSH ile bağlanma + kullanıcı sertleştirme** (kendi bilgisayardan, WSL veya PowerShell terminalinden):
   ```bash
   ssh root@<sunucu-ip>
   adduser cagri
   usermod -aG sudo cagri
   # /etc/ssh/sshd_config içinde PermitRootLogin no yapıp sshd'yi restart edin
   ```
   Bundan sonra: `ssh cagri@<sunucu-ip>`.

2. Adım 0-6 birebir aynı şekilde **bu SSH oturumu içinde** tekrarlanır. Tek fark: `deploy.sh` artık yerelde değil, uzaktan çalıştırılır:
   ```bash
   # Kendi bilgisayarınızdan (repo kökünden):
   DEPLOY_HOST=<sunucu-ip> DEPLOY_USER=cagri ./deploy/deploy.sh
   ```

3. `deploy/nginx-hayatiarac.conf` içindeki `server_name _;` satırı gerçek domain ile değiştirilir: `server_name yourdomain.com;`

4. Domain'in DNS A kaydı VPS IP'sine yönlendirilir (sağlayıcı panelinden, kullanıcı tarafından).

5. Let's Encrypt SSL:
   ```bash
   sudo apt-get install -y certbot python3-certbot-nginx
   sudo certbot --nginx -d yourdomain.com
   sudo certbot renew --dry-run   # otomatik yenileme timer'ının çalıştığını doğrula
   ```

6. `sudo ufw allow OpenSSH` artık çalışır (VPS'te `openssh-server` kurulu gelir) — `22/tcp` yerine bu kullanılabilir, fark etmez.

---

## Dosya haritası

| Dosya | Amaç |
|---|---|
| `deploy/hayatiarac-api.service` | systemd unit — `EnvironmentFile` ile sırları dışarıdan okur |
| `deploy/hayatiarac-api.env.example` | Gerçek `.env` dosyasının şablonu (sırlar burada **yok**) |
| `deploy/nginx-hayatiarac.conf` | Nginx reverse proxy config (Aşama A: SSL'siz; certbot Aşama B'de günceller) |
| `deploy/deploy.sh` | publish + kopyalama (local veya uzak) + servis restart otomasyonu |
| `HayatiArac.Api/appsettings.Production.json` | Production'a özel, sır içermeyen ayarlar (Serilog vb.) |
| `deploy/README-deploy.md` | Bu dosya |

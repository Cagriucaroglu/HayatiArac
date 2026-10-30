#!/usr/bin/env bash
# HayatiArac API deploy script.
#
# Aşama A (WSL provası): DEPLOY_HOST=local (varsayılan) ile, WSL dosya sistemine
# doğrudan kopyalar (cp).
# Aşama B (gerçek VPS): DEPLOY_HOST=<sunucu-ip-veya-domain> ve DEPLOY_USER
# ortam değişkenlerini set edip çalıştırın; rsync+ssh ile uzak sunucuya kopyalar.
#
# Kullanım (WSL/git-bash üzerinden, repo kökünden):
#   ./deploy/deploy.sh                                   # Aşama A, local
#   DEPLOY_HOST=1.2.3.4 DEPLOY_USER=cagri ./deploy/deploy.sh   # Aşama B, uzak sunucu
#
# Windows'ta publish adımı (dotnet) Windows tarafında çalışır; script'in geri
# kalanı (cp/rsync/systemctl) WSL veya hedef Linux sunucuda çalışmalıdır.

set -euo pipefail

PROJECT="HayatiArac.Api/HayatiArac.Api.csproj"
PUBLISH_DIR="publish"
REMOTE_APP_DIR="/opt/hayatiarac"
SERVICE_NAME="hayatiarac-api"

DEPLOY_HOST="${DEPLOY_HOST:-local}"
DEPLOY_USER="${DEPLOY_USER:-cagri}"

echo "==> 1) dotnet publish (linux-x64, framework-dependent)"
dotnet publish "$PROJECT" -c Release -r linux-x64 --self-contained false -o "$PUBLISH_DIR"

if [ "$DEPLOY_HOST" = "local" ]; then
    echo "==> 2) Yerel kopyalama (Aşama A — WSL): $PUBLISH_DIR -> $REMOTE_APP_DIR"
    sudo mkdir -p "$REMOTE_APP_DIR"
    sudo cp -r "$PUBLISH_DIR"/* "$REMOTE_APP_DIR"/

    echo "==> 3) Servisi yeniden başlatma"
    sudo systemctl restart "$SERVICE_NAME"
    sudo systemctl status "$SERVICE_NAME" --no-pager
else
    echo "==> 2) Uzak sunucuya kopyalama (Aşama B): $PUBLISH_DIR -> $DEPLOY_USER@$DEPLOY_HOST:$REMOTE_APP_DIR"
    rsync -avz --delete "$PUBLISH_DIR"/ "$DEPLOY_USER@$DEPLOY_HOST:$REMOTE_APP_DIR"/

    echo "==> 3) Uzak sunucuda servisi yeniden başlatma"
    ssh "$DEPLOY_USER@$DEPLOY_HOST" "sudo systemctl restart $SERVICE_NAME && sudo systemctl status $SERVICE_NAME --no-pager"
fi

echo "==> Tamamlandı."

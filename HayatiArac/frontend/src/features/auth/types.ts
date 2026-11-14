// Backend: HayatiArac.Modules.User.Application/DTOs/UserProfileDto.cs
export type UserProfile = {
  userId: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  city: string | null
  district: string | null
  avatarUrl: string | null
  bio: string | null
  createdAt: string
}

// Backend: Commands/Register/RegisterUserCommand.cs → RegisterUserResponse
export type RegisterResponse = {
  userId: string
  email: string
}

export type OtpChannel = 'email' | 'phone'

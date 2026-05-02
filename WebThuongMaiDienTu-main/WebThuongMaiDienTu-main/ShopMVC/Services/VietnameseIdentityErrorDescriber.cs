using Microsoft.AspNetCore.Identity;

namespace ShopMVC.Services
{
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = "Da xay ra loi khong xac dinh." };
        public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = "Du lieu vua thay doi, vui long thu lai." };
        public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = "Mat khau khong chinh xac." };
        public override IdentityError InvalidToken() => new() { Code = nameof(InvalidToken), Description = "Ma xac thuc khong hop le." };
        public override IdentityError LoginAlreadyAssociated() => new() { Code = nameof(LoginAlreadyAssociated), Description = "Tai khoan dang nhap nay da duoc lien ket voi nguoi dung khac." };
        public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = $"Ten dang nhap '{userName}' khong hop le." };
        public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = $"Email '{email}' khong hop le." };
        public override IdentityError DuplicateUserName(string? userName) => new() { Code = nameof(DuplicateUserName), Description = $"Ten dang nhap '{userName}' da ton tai." };
        public override IdentityError DuplicateEmail(string? email) => new() { Code = nameof(DuplicateEmail), Description = $"Email '{email}' da duoc su dung." };
        public override IdentityError InvalidRoleName(string? role) => new() { Code = nameof(InvalidRoleName), Description = $"Vai tro '{role}' khong hop le." };
        public override IdentityError DuplicateRoleName(string? role) => new() { Code = nameof(DuplicateRoleName), Description = $"Vai tro '{role}' da ton tai." };
        public override IdentityError UserAlreadyHasPassword() => new() { Code = nameof(UserAlreadyHasPassword), Description = "Tai khoan nay da co mat khau." };
        public override IdentityError UserLockoutNotEnabled() => new() { Code = nameof(UserLockoutNotEnabled), Description = "Tinh nang khoa tai khoan chua duoc bat." };
        public override IdentityError UserAlreadyInRole(string? role) => new() { Code = nameof(UserAlreadyInRole), Description = $"Nguoi dung da thuoc vai tro '{role}'." };
        public override IdentityError UserNotInRole(string? role) => new() { Code = nameof(UserNotInRole), Description = $"Nguoi dung khong thuoc vai tro '{role}'." };
        public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"Mat khau phai co it nhat {length} ky tu." };
        public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mat khau phai co it nhat mot ky tu dac biet." };
        public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = "Mat khau phai co it nhat mot chu so." };
        public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = "Mat khau phai co it nhat mot chu thuong." };
        public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = "Mat khau phai co it nhat mot chu in hoa." };
    }
}

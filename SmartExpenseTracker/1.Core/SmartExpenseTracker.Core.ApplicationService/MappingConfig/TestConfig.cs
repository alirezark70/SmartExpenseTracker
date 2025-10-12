using Mapster;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
namespace SmartExpenseTracker.Core.ApplicationService.MappingConfig
{
    public class TestConfig : IMappingConfig
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Test, TestDto>()
            .IgnoreNullValues(true);
        }
    }


    public class TestDto
    {
        public int Id { get; set; }
    }

    public class Test
    {
        public int Id { get; set; }
    }

    //public class ComprehensiveMappingConfig : IMappingConfig
    //{
    //    public void Register(TypeAdapterConfig config)
    //    {
    //        config.NewConfig<User, UserDto>()

    //            // ===== 1. نادیده گرفتن مقادیر Null =====
    //            // وقتی true باشد، اگر property در Source مقدار null داشته باشد، 
    //            // مقدار Destination تغییر نمی‌کند
    //            .IgnoreNullValues(true)

    //            // ===== 2. Map کردن با نام‌های متفاوت (Property Mapping) =====
    //            // وقتی نام property ها متفاوت است
    //            .Map(dest => dest.UserId, src => src.Id)
    //            .Map(dest => dest.Phone, src => src.PhoneNumber)

    //            // ===== 3. ترکیب چند Property (Concatenation) =====
    //            // ترکیب FirstName و LastName به FullName
    //            .Map(dest => dest.FullName,
    //                 src => $"{src.FirstName} {src.LastName}")

    //            // ===== 4. محاسبات سفارشی (Custom Calculations) =====
    //            // محاسبه سن بر اساس تاریخ تولد
    //            .Map(dest => dest.Age,
    //                 src => DateTime.Now.Year - src.BirthDate.Year)

    //            // ===== 5. Map کردن Nested Objects =====
    //            // Map کردن یک object تو در تو
    //            .Map(dest => dest.AddressInfo, src => src.Address)

    //            // ===== 6. Flattening - صاف کردن Nested Properties =====
    //            // دسترسی به property های درون object دیگر
    //            .Map(dest => dest.FullAddress,
    //                 src => $"{src.Address.Street}, {src.Address.City}, {src.Address.Country}")

    //            // ===== 7. کار با Collections =====
    //            // شمارش تعداد آیتم‌ها
    //            .Map(dest => dest.TotalOrders,
    //                 src => src.Orders != null ? src.Orders.Count : 0)

    //            // محاسبه روی Collection
    //            .Map(dest => dest.TotalOrderAmount,
    //                 src => src.Orders != null ? src.Orders.Sum(o => o.Amount) : 0)

    //            // ===== 8. Map کردن Enum ها =====
    //            // تبدیل Enum به String
    //            .Map(dest => dest.UserTypeName,
    //                 src => src.Type.ToString())

    //            // ===== 9. Conditional Mapping - Map شرطی =====
    //            // اگر Salary مقدار داشت نمایش بده، وگرنه "نامشخص"
    //            .Map(dest => dest.SalaryDisplay,
    //                 src => src.Salary.HasValue
    //                     ? $"{src.Salary.Value:N0} تومان"
    //                     : "نامشخص")

    //            // Map بر اساس شرط
    //            .Map(dest => dest.Status,
    //                 src => src.IsActive ? "فعال" : "غیرفعال")

    //            // ===== 10. نادیده گرفتن Property خاص =====
    //            // این property اصلاً map نمی‌شود
    //            .Ignore(dest => dest.Status)

    //            // ===== 11. Map دو طرفه (Reverse Mapping) =====
    //            // اگر بخواهید از UserDto به User هم map کنید
    //            .TwoWays()

    //            // ===== 12. AfterMapping - اجرای کد بعد از Map =====
    //            // بعد از Map شدن، می‌توانید تغییرات دستی اعمال کنید
    //            .AfterMapping((src, dest) =>
    //            {
    //                // مثلاً چک کنیم اگر ایمیل خالی بود، یک مقدار پیش‌فرض بدهیم
    //                if (string.IsNullOrEmpty(dest.Email))
    //                {
    //                    dest.Email = "no-email@example.com";
    //                }
    //            })

    //            // ===== 13. BeforeMapping - اجرای کد قبل از Map =====
    //            .BeforeMapping((src, dest) =>
    //            {
    //                // کارهایی که قبل از map باید انجام شود
    //                Console.WriteLine($"Mapping user: {src.FirstName}");
    //            })

    //            // ===== 14. PreserveReference - حفظ Reference ها =====
    //            // برای جلوگیری از Circular Reference در object های پیچیده
    //            .PreserveReference(true)

    //            // ===== 15. MaxDepth - عمق Map کردن =====
    //            // حداکثر عمق Map کردن object های تو در تو
    //            .MaxDepth(3)

    //            // ===== 16. ConstructUsing - استفاده از Constructor سفارشی =====
    //            // اگر بخواهید خودتان instance بسازید
    //            .ConstructUsing(src => new UserDto
    //            {
    //                UserId = src.Id,
    //                Email = src.Email?.ToLower()
    //            })

    //            // ===== 17. IgnoreAttribute - نادیده گرفتن بر اساس Attribute =====
    //            .IgnoreMember((member, side) =>
    //                member.HasCustomAttribute(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute)))

    //            // ===== 18. ShallowCopy - کپی سطحی =====
    //            // فقط reference ها کپی می‌شوند نه خود object ها
    //            .ShallowCopy(true);


    //        // ===== 19. کانفیگ Global - اعمال به همه Map ها =====
    //        config.Default
    //            .IgnoreNullValues(true)
    //            .PreserveReference(true);


    //        // ===== 20. Map کردن با شرط (MapWith) =====
    //        config.NewConfig<User, UserDto>()
    //            .Map(dest => dest.FullName,
    //                 src => src.FirstName + " " + src.LastName,
    //                 srcCond => srcCond.FirstName != null && srcCond.LastName != null);


    //        // ===== 21. IgnoreIf - نادیده گرفتن شرطی =====
    //        config.NewConfig<User, UserDto>()
    //            .Map(dest => dest.Phone, src => src.PhoneNumber)
    //            .IgnoreIf((src, dest) => string.IsNullOrEmpty(src.PhoneNumber));


    //        // ===== 22. Forking - ایجاد کانفیگ مشتق =====
    //        // ساخت کانفیگ جدید از روی کانفیگ موجود
    //        config.ForType<User, UserDto>()
    //            .Fork(forked => forked
    //                .Map(dest => dest.Email, src => src.Email.ToUpper()));
    //    }
    //}


}

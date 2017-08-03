using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TestTask.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TestTask.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {

            if (db.ResultModels.Count() == 0)
            {
                CreateRole();
                CreateUsersStudent();
                CreateTeacherWithDekan();
                CreateSubject();
                CreateSubjectModelTableTeacher();
                CreateSubjectModelTableStudent();
                CreateForResultTable();
            }

            db.SaveChanges();

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { Name = model.Name, Email = model.Email, UserName = model.Email, Surname = model.Surname, Type = model.Type };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, user.Type);
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }        

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
       
        public void CreateUsersStudent()
        {
            List<string> Name = new List<string>();
            List<string> Surname = new List<string>();

            Name.Add("Петр"); Name.Add("Анатолий"); Name.Add("Филипп");
            Name.Add("Эмма"); Name.Add("Виталий"); Name.Add("Трофим");
            Name.Add("Наталия"); Name.Add("Вальтер"); Name.Add("Давид");
            Name.Add("Флора"); Name.Add("Владимир"); Name.Add("Боримир");
            Name.Add("Вероника"); Name.Add("Аристарх"); Name.Add("Савва");
            Name.Add("Юлиана"); Name.Add("Христофор"); Name.Add("Олег");
            Name.Add("Викторин"); Name.Add("Максим"); Name.Add("Леонард");
            Name.Add("Лариса"); Name.Add("Модест"); Name.Add("Игорь");
            Name.Add("Елена"); Name.Add("Владислав"); Name.Add("Оскар");
            Name.Add("Ольга"); Name.Add("Артём"); Name.Add("Адам");
            Name.Add("Мария"); Name.Add("Виктор"); Name.Add("Фёдор");
            Name.Add("Тамара"); Name.Add("Захар"); Name.Add("Григорий");
            Name.Add("Лада"); Name.Add("Юлий"); Name.Add("Серафим");
            Name.Add("Милена"); Name.Add("Герман"); Name.Add("Джон");
            Name.Add("Фаина"); Name.Add("Никита"); Name.Add("Савва");
            Name.Add("Инна"); Name.Add("Влад"); Name.Add("Игорь");
            Name.Add("Анастасия"); Name.Add("Леонид"); Name.Add("Гарри");
            Name.Add("Болеслава"); Name.Add("Джозеф"); Name.Add("Аида");
            Name.Add("Юнона"); Name.Add("Ян"); Name.Add("Ева");
            Name.Add("Арина"); Name.Add("Артур"); Name.Add("Клавдия");
            Name.Add("Рада"); Name.Add("Григорий"); Name.Add("Галена");
            Name.Add("Ева"); Name.Add("Джон"); Name.Add("Нина");
            Name.Add("Октябрина"); Name.Add("Евгений"); Name.Add("Галина");
            Name.Add("Лада"); Name.Add("Савелий"); Name.Add("Евдокия");
            Name.Add("Василиса"); Name.Add("Вальтер"); Name.Add("Катерина");
            Name.Add("Алиса"); Name.Add("Джон"); Name.Add("Роза");
            Name.Add("Эльза"); Name.Add("Антонин"); Name.Add("Дарина");
            Name.Add("Августина"); Name.Add("Игнатий"); Name.Add("Оксана");
            Name.Add("Алина"); Name.Add("Джозеф"); Name.Add("Светослава");
            Name.Add("Джульетта"); Name.Add("Григорий"); Name.Add("Кристина");

            Surname.Add("Петрович"); Surname.Add("Сафонов"); Surname.Add("Корсунов");
            Surname.Add("Шашкова"); Surname.Add("Туров"); Surname.Add("Капустин");
            Surname.Add("Татаринова"); Surname.Add("Самойлов"); Surname.Add("Белоусов");
            Surname.Add("Блохина"); Surname.Add("Русаков"); Surname.Add("Зюганов");
            Surname.Add("Захарова"); Surname.Add("Кузнецов"); Surname.Add("Уваров");
            Surname.Add("Мамонтова"); Surname.Add("Кудрявцев"); Surname.Add("Игнатов");
            Surname.Add("Шубина"); Surname.Add("Никитин"); Surname.Add("Голубев");
            Surname.Add("Ершова"); Surname.Add("Шаров"); Surname.Add("Шарапов");
            Surname.Add("Голубева"); Surname.Add("Степанов"); Surname.Add("Котов");
            Surname.Add("Кузьмина"); Surname.Add("Аксёнов"); Surname.Add("Жириновский");
            Surname.Add("Антонова"); Surname.Add("Фёдоров"); Surname.Add("Баранов");
            Surname.Add("Лапшова"); Surname.Add("Кудряшов"); Surname.Add("Поляков");
            Surname.Add("Ковалёва"); Surname.Add("Лебедев"); Surname.Add("Корсунов");
            Surname.Add("Хохлова"); Surname.Add("Сорокин"); Surname.Add("Колесов");
            Surname.Add("Крылова"); Surname.Add("Игнатьев"); Surname.Add("Лыткин");
            Surname.Add("Третьякова"); Surname.Add("Комиссаров"); Surname.Add("Павлов");
            Surname.Add("Колесова"); Surname.Add("Семёнов"); Surname.Add("Кулаков");
            Surname.Add("Колесова"); Surname.Add("Стрелков"); Surname.Add("Савельева");
            Surname.Add("Орехова"); Surname.Add("Мельников"); Surname.Add("Горбачёва");
            Surname.Add("Артемьева"); Surname.Add("Ларионов"); Surname.Add("Носкова");
            Surname.Add("Алефанова"); Surname.Add("Некрасов"); Surname.Add("Гришина");
            Surname.Add("Андреева"); Surname.Add("Сорокин"); Surname.Add("Логинова");
            Surname.Add("Смирнова"); Surname.Add("Сазонов"); Surname.Add("Владимирова");
            Surname.Add("Потапова"); Surname.Add("Яковлев"); Surname.Add("Носова");
            Surname.Add("Суханова"); Surname.Add("Гаврилов"); Surname.Add("Некрасова");
            Surname.Add("Сидорова"); Surname.Add("Беляев"); Surname.Add("Алексеева");
            Surname.Add("Пестова"); Surname.Add("Рогов"); Surname.Add("Рыбакова");
            Surname.Add("Афанасьева"); Surname.Add("Селиверстов"); Surname.Add("Колесникова");
            Surname.Add("Дьячкова"); Surname.Add("Сорокин"); Surname.Add("Иванкова");
            Surname.Add("Щербакова"); Surname.Add("Кулаков"); Surname.Add("Воробьёва");
            
            int emailstep = 0;
            for (int i = 0; i < Surname.Count; i++)
            {
                CreateUserWithRole( new ApplicationUser() { Name = Name[i], Surname = Surname[i], Email = "Petr" + emailstep + "@mail.ru", UserName = "Petr" + emailstep + "@mail.ru", Type = "Student"},  "1234567" );
                emailstep++;
            }
        }

        public void CreateUserWithRole(ApplicationUser User, string pass)
        {
            var user = UserManager.Create(User, pass);
            if (user.Succeeded)
            {
                UserManager.AddToRole(User.Id, User.Type);
            }
        }

        public void CreateSubject()
        {
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Анализ и моделирование финансово-экономических систем" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Антикризисное управление" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Безопасность жизнедеятельности" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Бухгалтерский учет и анализ" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Государственное регулирование экономикой" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Деньги, кредит, банки" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Инвестиционный анализ" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Иностранный язык" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Институциональная экономика" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "История" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "История культуры Урала" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Культурология" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Линейная алгебра" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Логика" });
            db.AllSubjectModels.Add(new AllSubjectModel { Subject = "Макроэкономика" });

            db.SaveChanges();
        }

        public void CreateTeacherWithDekan()
        {

            CreateUserWithRole(new ApplicationUser() { Name = "Серафим", Surname = "Петухов", Email = "Petr" + 452 + "@mail.ru", UserName = "Petr" + 452 + "@mail.ru", Type = "Teacher" },"1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Назар", Surname = "Симонов", Email = "Petr" + 453 + "@mail.ru", UserName = "Petr" + 453 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Яков", Surname = "Ермаков", Email = "Petr" + 454 + "@mail.ru", UserName = "Petr" + 454 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Давид", Surname = "Гаскаров", Email = "Petr" + 455 + "@mail.ru", UserName = "Petr" + 455 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Вилен", Surname = "Князев", Email = "Petr" + 456 + "@mail.ru", UserName = "Petr" + 456 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Анатолий", Surname = "Кудрявцев", Email = "Petr" + 457 + "@mail.ru", UserName = "Petr" + 457 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Светлан", Surname = "Фомичёв", Email = "Petr" + 458 + "@mail.ru", UserName = "Petr" + 458 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Филипп", Surname = "Крылов", Email = "Petr" + 459 + "@mail.ru", UserName = "Petr" + 459 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Нина", Surname = "Елисеева", Email = "Petr" + 460 + "@mail.ru", UserName = "Petr" + 460 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Валерия", Surname = "Тимошенко", Email = "Petr" + 461 + "@mail.ru", UserName = "Petr" + 461 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Млада", Surname = "Воронова", Email = "Petr" + 462 + "@mail.ru", UserName = "Petr" + 462 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Валентина", Surname = "Чиркина", Email = "Petr" + 463 + "@mail.ru", UserName = "Petr" + 463 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Василиса", Surname = "Рябова", Email = "Petr" + 464 + "@mail.ru", UserName = "Petr" + 464 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Авдотья", Surname = "Игнатьева", Email = "Petr" + 465 + "@mail.ru", UserName = "Petr" + 465 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Надежда", Surname = "Лебедева", Email = "Petr" + 466 + "@mail.ru", UserName = "Petr" + 466 + "@mail.ru", Type = "Teacher" }, "1234567");
            CreateUserWithRole(new ApplicationUser() { Name = "Надежда", Surname = "Лебедева", Email = "Petr" + 1000 + "@mail.ru", UserName = "Petr" + 1000 + "@mail.ru", Type = "Dekan"}, "1234567");
        }

        public void CreateSubjectModelTableStudent()
        {
            Random random = new Random();
            AllSubjectModel sub;
            int steprandom;
           foreach(var user in UserManager.Users.ToList())
            {
                if (user.Type == "Student")
                {
                    steprandom = random.Next(0, 15);
                    sub = db.AllSubjectModels.ToList()[steprandom];
                    db.SubjectModels.Add(new SubjectModel { SubjectId = sub.Id, UserId = user.Id, Type = user.Type });
                }
            }
            db.SaveChanges();
        }

        public void CreateSubjectModelTableTeacher()
        {
            AllSubjectModel sub;
            int step = 0;
            foreach (var user in UserManager.Users)
            {
                if (user.Type == "Teacher" && step < 15)
                {
                    sub = db.AllSubjectModels.ToList()[step];
                    db.SubjectModels.Add(new SubjectModel { SubjectId = sub.Id, UserId = user.Id, Type = user.Type });
                    step++;
                }
            }
            db.SaveChanges();
        }

        public void CreateForResultTable()
        {
            Random random = new Random();
            foreach (var sub in db.SubjectModels.ToList())
            {
                if (sub.Type == "Student")
                {
                    db.ResultModels.Add(new ResultModel { SubjectID = sub.Id, UserId = sub.UserId, Score = random.Next(1, 6) });
                }
            }
            db.SaveChanges();
        }

        public void CreateRole()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new IdentityDbContext()));
            roleManager.Create(new IdentityRole { Name = "Student"});
            roleManager.Create(new IdentityRole { Name = "Teacher" });
            roleManager.Create(new IdentityRole { Name = "Dekan" });
            db.SaveChanges();
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
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

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
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
                var user = new ApplicationUser { Name = model.Name, Email = model.Email, Surname = model.Surname, Type = model.Type };
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
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
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

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
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
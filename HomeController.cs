using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tennis.Models;
using Tennis.ViewModels;

namespace Tennis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersContext _db;
        public HomeController(ILogger<HomeController> logger, UsersContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pagesize = 1000;
            var users = _db.ContextUsers.Where(x => x.Points > 0).OrderBy(x => x.Points).ToList();
            var tournaments = _db.Tournaments.OrderBy(x => x.Date).Where(x => x.Status == StatusTournament.Will).ToList();
            var _users = _db.ContextUsers.Where(x => x.Points > 0).OrderBy(x => x.Points).ToList();
            var tournamets = _db.Tournaments.OrderByDescending(x => x.Date).ToList();
            int numberOfrecords = 5;
            var lastFiveNews = _db.News.OrderByDescending(x => x.DateOfPublication).Take(numberOfrecords).ToList();
            IQueryable<Sponsor> sponsors = _db.Sponsors.OrderBy(x => x.DateCreate);
            var count = await sponsors.CountAsync();
            var items = await sponsors.Skip((page - 1) * pagesize).Take(pagesize).ToListAsync();
            PageViewModel pageViewModel = new PageViewModel(count, page, pagesize);

            GetTopFiveRatingList(out List<Rating> topFiveManOneProMasters,
            out List<Rating> topFiveManOneMasters,
            out List<Rating> topFiveManOneFutures,
            out List<Rating> topFiveManTwoProMasters,
            out List<Rating> topFiveManTwoMasters,
            out List<Rating> topFiveManTwoFutures,
            out List<Rating> topFiveWomanOneMasters,
            out List<Rating> topFiveWomanOneFutures,
            out List<Rating> topFiveWomanTwoMasters,
            out List<Rating> topFiveWomanTwoFutures,
            out List<Rating> topFiveMixedProMasters,
            out List<Rating> topFiveMixedMasters,
            out List<Rating> topFiveMixedFutures);

            var userList = _db.Users.ToList();
            TopFiveRatingViewModel topFiveRatingViewModel = new TopFiveRatingViewModel
            {
                TopFiveManOneProMasters = topFiveManOneProMasters,
                TopFiveManOneMasters = topFiveManOneMasters,
                TopFiveManOneFutures = topFiveManOneFutures,
                TopFiveManTwoProMasters = topFiveManTwoProMasters,
                TopFiveManTwoMasters = topFiveManTwoMasters,
                TopFiveManTwoFutures = topFiveManTwoFutures,
                TopFiveWomanOneMasters = topFiveWomanOneMasters,
                TopFiveWomanOneFutures = topFiveWomanOneFutures,
                TopFiveWomanTwoMasters = topFiveWomanTwoMasters,
                TopFiveWomanTwoFutures = topFiveWomanTwoFutures,
                TopFiveMixedProMasters = topFiveMixedProMasters,
                TopFiveMixedMasters = topFiveMixedMasters,
                TopFiveMixedFutures = topFiveMixedFutures
            };

            MainViewModel ivm = new MainViewModel()
            {
                Users = _users,
                Tournaments = tournaments,
                Sponsorss = sponsors,
                PageViewModelSponsor = pageViewModel,
                Sponsors = items,
                LastNews = lastFiveNews,
                TopFiveRatingViewModel = topFiveRatingViewModel
            };
            return View(ivm);
        }
        [HttpGet]
        public FileResult GetVirtualFile(int key)
        {
            if (key < 2 || key > 6)
                throw new FileNotFoundException("Документ не найден!");
            Dictionary<int, string> keyValuesDocument = new Dictionary<int, string>
            {
                {2, "pol_KSLT.pdf" },
                {3, "form_application_tournament.pdf" },
                {4, "rating_points_table.pdf" },
                {5, "dis_pol_KSLT.pdf" },
                {6, "public_offer.pdf" }
            };
            ViewData["TitleDocumentPage"] = keyValuesDocument[key];
            var filepath = Path.Combine("wwwroot/avatars", keyValuesDocument[key]);
            byte[] FileBytes = System.IO.File.ReadAllBytes(filepath);
            return File(FileBytes,  "application/pdf");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult GetTopRating(Models.Type type, CategoryPerson level)
        {
            var ratingViewModel = new RatingViewModel()
            {
                Ratings = _db.Ratings.Where(r => r.Type == type && r.Level == level).OrderByDescending(r => r.Points).Take(10).ToList()
            };
            var usersList = new List<User>();
            foreach (var item in ratingViewModel.Ratings)
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == item.UserId);
                usersList.Add(user);
            };
            ratingViewModel.Users = usersList;
            return Json(new { ratingViewModel });
        }

        public void GetTopFiveRatingList(
            out List<Rating> topFiveManOneProMasters,
            out List<Rating> topFiveManOneMasters,
            out List<Rating> topFiveManOneFutures,
            out List<Rating> topFiveManTwoProMasters,
            out List<Rating> topFiveManTwoMasters,
            out List<Rating> topFiveManTwoFutures,
            out List<Rating> topFiveWomanOneMasters,
            out List<Rating> topFiveWomanOneFutures,
            out List<Rating> topFiveWomanTwoMasters,
            out List<Rating> topFiveWomanTwoFutures,
            out List<Rating> topFiveMixedProMasters,
            out List<Rating> topFiveMixedMasters,
            out List<Rating> topFiveMixedFutures)
        {
            topFiveManOneProMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.ManOne && x.Level == CategoryPerson.Promasters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveManOneMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.ManOne && x.Level == CategoryPerson.Masters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveManOneFutures = _db.Ratings
                .Where(x => x.Type == Models.Type.ManOne && x.Level == CategoryPerson.Futures && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveManTwoProMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.ManTwo && x.Level == CategoryPerson.Promasters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveManTwoMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.ManTwo && x.Level == CategoryPerson.Masters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveManTwoFutures = _db.Ratings
                .Where(x => x.Type == Models.Type.ManTwo && x.Level == CategoryPerson.Futures && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveWomanOneMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.WomenOne && x.Level == CategoryPerson.Masters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveWomanOneFutures = _db.Ratings
                .Where(x => x.Type == Models.Type.WomenOne && x.Level == CategoryPerson.Futures && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveWomanTwoMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.WomenTwo && x.Level == CategoryPerson.Masters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveWomanTwoFutures = _db.Ratings
                .Where(x => x.Type == Models.Type.WomenTwo && x.Level == CategoryPerson.Futures && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveMixedProMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.Mixed && x.Level == CategoryPerson.Promasters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveMixedMasters = _db.Ratings
                .Where(x => x.Type == Models.Type.Mixed && x.Level == CategoryPerson.Masters && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();

            topFiveMixedFutures = _db.Ratings
                .Where(x => x.Type == Models.Type.Mixed && x.Level == CategoryPerson.Futures && x.RatingActivation)
                .OrderByDescending(x => x.Points).Take(5)
                .ToList();
        }

    }
}

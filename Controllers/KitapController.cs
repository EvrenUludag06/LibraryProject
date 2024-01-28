﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebUygulamaProje1.Models;
using WebUygulamaProje1.Utility;

namespace WebUygulamaProje1.Controllers
{
	
    public class KitapController : Controller
    {
        private readonly IKitapRepository _kitapRepository;
		private readonly IKitapTuruRepository _kitapTuruRepository;
		public readonly IWebHostEnvironment _webHostEnvironment;

		public KitapController(IKitapRepository kitapRepository, IKitapTuruRepository kitapTuruRepository, IWebHostEnvironment webHostEnvironment)
		{
			_kitapRepository = kitapRepository;
			_kitapTuruRepository = kitapTuruRepository;
			_webHostEnvironment = webHostEnvironment;
		}


		[Authorize(Roles = "Admin,Ogrenci")]		
		public IActionResult Index()
        {			
			List<Kitap> objKitapList = _kitapRepository.GetAll(includeProps:"KitapTuru").ToList();
			return View(objKitapList);
        }


        // Get
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult EkleGuncelle(int? id)
        {
			IEnumerable<SelectListItem> KitapTuruList = _kitapTuruRepository.GetAll()
				.Select(k => new SelectListItem
				{
					Text = k.Ad,
					Value = k.Id.ToString()
				});
			ViewBag.KitapTuruList = KitapTuruList;

			if (id == null || id==0)
			{
				// ekle
				return View();
			}
			else
			{
				// guncelleme
				Kitap? kitapVt = _kitapRepository.Get(u => u.Id == id);  // Expression<Func<T, bool>> filtre
				if (kitapVt == null)
				{
					return NotFound();
				}
				return View(kitapVt);
			}
			
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult EkleGuncelle(Kitap kitap, IFormFile? file)
		{
			var errors = ModelState.Values.SelectMany(v => v.Errors);

			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				string kitapPath = Path.Combine(wwwRootPath, @"img");

				if (file != null)
				{
					using (var fileStream = new FileStream(Path.Combine(kitapPath, file.FileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					kitap.ResimUrl = @"\img\" + file.FileName;
				}				
								
				if (kitap.Id == 0)
				{
					_kitapRepository.Ekle(kitap);
					TempData["basarili"] = "Yeni Kitap başarıyla oluşturuldu!";
				}
				else
				{
					_kitapRepository.Guncelle(kitap);
					TempData["basarili"] = "Kitap güncelleme başarılı!";
				}
				
				_kitapRepository.Kaydet(); // SaveChanges() yapmazsanız bilgiler veri tabanına eklenmez!			
                return RedirectToAction("Index", "Kitap");
            }
            return View();                                 
		}



        // GET ACTION
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult Sil(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Kitap? kitapVt = _kitapRepository.Get(u => u.Id == id);
			if (kitapVt == null)
			{
				return NotFound();
			}
			return View(kitapVt);
		}


		[HttpPost, ActionName("Sil")]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult SilPOST(int? id)		
		{
			Kitap? kitap = _kitapRepository.Get(u => u.Id == id);
			if (kitap == null)
			{
				return NotFound();
			}
			_kitapRepository.Sil(kitap);
			_kitapRepository.Kaydet();
			TempData["basarili"] = "Kayıt Silme işlemi başarılı!";
			return RedirectToAction("Index", "Kitap");
		}
	}
}

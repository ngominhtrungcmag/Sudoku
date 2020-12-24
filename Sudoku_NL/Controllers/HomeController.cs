using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Sudoku_NL.Models;

namespace Sudoku_NL.Controllers
{
    public class HomeController : Controller
    {
        // GET: Sudoku
        public ActionResult Index()
        {
            return View(model: SudokuModels.SudokuGame);
        }

        private bool WithinRange(int num)
        {
            return num >= 0 && num < 9;
        }

        [HttpGet]
        public string GetSelectedCell(int i, int j)
        {
            if (WithinRange(i) && WithinRange(j))
            {
                SudokuModels.SudokuGame.UpdatePossible();
                SudokuModels.SudokuGame.SelectedCell.CopyCell(SudokuModels.SudokuGame.Cells[i, j]);
                Console.WriteLine("Message form getselectedcell: " + SudokuModels.SudokuGame.SelectedCell.Id);
            }
            return SudokuModels.SudokuGame.SelectedCell.PossibleString;
        }

        [HttpGet]
        public bool CheckOneCell()
        {
            int i, j;
            i = SudokuModels.SudokuGame.SelectedCell.Row;
            j = SudokuModels.SudokuGame.SelectedCell.Column;
            Console.WriteLine("Message from home Controller CheckOneCell:" + SudokuModels.SudokuGame.Cells[i, j].Value);
            return (SudokuModels.SudokuGame.CheckOneCell(i, j));
        }

        //Nhận giá trị từ client
        [HttpPost]
        public ActionResult GetCellValue(int i, int j, string val)
        {
            int value = Int32.Parse(val);
            {
                Console.WriteLine(val);
                SudokuModels.SudokuGame.Cells[i, j].Value = value;
                SudokuModels.SudokuGame.UpdatePossible();
                SudokuModels.SudokuGame.SelectedCell.CopyCell(SudokuModels.SudokuGame.Cells[i, j]);
                Console.WriteLine(SudokuModels.SudokuGame.SelectedCell.Id);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult PostNewGame(int difficult)
        {
            SudokuModels.SudokuGame.NewGame(difficult);
            return RedirectToAction("Index");
        }

        #region File upload
        private IWebHostEnvironment iwebHostEnvironment;
        public HomeController(IWebHostEnvironment _iwebHostEnvironment)
        {
            this.iwebHostEnvironment = _iwebHostEnvironment;
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            var filePath = Path.Combine(this.iwebHostEnvironment.WebRootPath, "text", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            SudokuModels.SudokuGame.LoadFile(filePath);
            return RedirectToAction("Index");
        }
        #endregion


        #region Down File
        [HttpGet]
        public ActionResult DownFile()
        {
            Console.WriteLine("Run DownFile");
            MemoryStream memoryStream = new MemoryStream();
            TextWriter tw = new StreamWriter(memoryStream);
            for (int i = 0; i < 9; i++)
            {
                string str = "";
                for (int j = 0; j < 9; j++)
                {
                    str += SudokuModels.SudokuGame.Cells[i, j].Value.ToString() + " ";
                }
                tw.WriteLine(str.Trim());
            }
            tw.Flush();

            var length = memoryStream.Length;
            tw.Close();
            var toWrite = new byte[length];
            Array.Copy(memoryStream.GetBuffer(), 0, toWrite, 0, length);

            return File(toWrite, "text/plain", "SaveSudoku.txt");
        }
        #endregion
        [HttpPost]
        public ActionResult ResetCell()
        {
            SudokuModels.SudokuGame.Reset();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public string PostSolve()
        {
            bool validResult = SudokuModels.SudokuGame.SolveGame();
            if (validResult)
            {
                return "Giải thành công!";
            }
            else
            {
                return "Giải không thành công!";
            }
        }

        [HttpPost]
        public string PostComplete()
        {
            bool validResult = SudokuModels.SudokuGame.CheckComplete();
            if (validResult)
            {
                return "Chúc mừng bạn đã giải đúng!";
            }
            else
            {
                return "Bạn giải sai rồi. Làm lại đi nhấn reset nào!";
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutAuthur()
        {
            return View();
        }

        public IActionResult UserManual()
        {
            return View();
        }

    }
}

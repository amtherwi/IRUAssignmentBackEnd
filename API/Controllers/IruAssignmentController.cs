using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.IruAssignment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
// using System.Net.Http;
// using System.Data;
// using ExcelDataReader;
using System.IO;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.Linq;
using System.Threading;
// using System.Text;
using EFCore.BulkExtensions;

namespace API.Controllers
{
    public class IruAssignmentController : BaseApiController
    {
        private readonly DataContext _context;
        private IWebHostEnvironment _hostingEnvironment;
        public IruAssignmentController(DataContext context, IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
            _context = context;
        }
        [HttpGet]
        public async Task<Response<List<IruAssignment>>> GetIruAssignments()
        {
            var list = new List<IruAssignment>();
            list = await _context.IruAssignments.ToListAsync();
            //if ((list != null) && (!list.Any()))
                return Response<List<IruAssignment>>.GetResult(200, "ok", list);

            //return Response<List<IruAssignment>>.GetResult(200, "No Elemenet!", null);

        }
        [HttpGet]
        [Route("items")]
        public ActionResult GetIruAssignmentsByItem()
        {
            var query = _context.IruAssignments
                   .GroupBy(p => p.ColorCode)
                   .Select(g => new { name = g.Key, count = g.Count() });
            return Ok(query);
        }

        [HttpGet("{id}")]
        public async Task<Response<IruAssignment>> GetIruAssignment(Guid Id)
        {
            var item = await _context.IruAssignments.FindAsync(Id);
            if (item != null)
                return Response<IruAssignment>.GetResult(200, "ok", item);
            else
                return Response<IruAssignment>.GetResult(404, "Not Found", null);
        }
        [HttpGet("items/{colorCode}")]
        public async Task<Response<List<IruAssignment>>> GetItemsByCode(string ColorCode)
        {
            // context.Students
            //            .where(s => s.StudentName == "Bill")
            //            .FirstOrDefault<Student>();  
            var items = new List<IruAssignment>();
            items =  await _context.IruAssignments
                        .Where( i => i.ColorCode == ColorCode).ToListAsync();
            
            return Response<List<IruAssignment>>.GetResult(200, "ok", items); 
        }
  
        // POST api/IruAssignment/import
        [HttpPost("import")]
        [RequestFormLimits(MultipartBodyLengthLimit = 299939492)] // 299MB for larg file import 
        [RequestSizeLimit(299939492)]
        public async Task<Response<List<IruAssignment>>> Import(IFormFile formFile, CancellationToken cancellationToken)
        {
            try
            {
                var list = new List<IruAssignment>();
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("importedData");
                var importedData = Path.Combine(_hostingEnvironment.WebRootPath, folderName);
                if (!Directory.Exists(importedData))
                {
                    Directory.CreateDirectory(importedData);
                }

                if (file == null || file.Length <= 0)
                {
                    return Response<List<IruAssignment>>.GetResult(-1, "formfile is empty");
                }

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return Response<List<IruAssignment>>.GetResult(-1, "Not Support file extension");
                }


                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string renameFile = Convert.ToString(Guid.NewGuid()) + ".json";
                var filePath = Path.Combine(importedData, renameFile);

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream, cancellationToken);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
        
                        var startRow = 2;
    
                        var rowCount = worksheet.Dimension.Rows;
                        IruAssignment obj;
                        for (int row = startRow; row <= rowCount; row++)
                        {
                            obj = new IruAssignment
                            {
                                Id = Guid.NewGuid(),
                                Key = worksheet.Cells[row, 1].Value.ToString().Trim(),
                                ItemCode = worksheet.Cells[row, 2].Value.ToString().Trim(),
                                ColorCode = worksheet.Cells[row, 3].Value.ToString().Trim(),
                                Description = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                Price = decimal.Parse(worksheet.Cells[row, 5].Value.ToString().Trim()),
                                DiscountPrice = decimal.Parse(worksheet.Cells[row, 6].Value.ToString().Trim()),
                                DeliveredIn = worksheet.Cells[row, 7].Value.ToString().Trim(),
                                Q1 = worksheet.Cells[row, 8].Value.ToString().Trim(),
                                Size = int.Parse(worksheet.Cells[row, 9].Value.ToString().Trim()),
                                Color = worksheet.Cells[row, 10].Value.ToString().Trim(),
                            };
                            list.Add(obj);

                        }
                        // insert bulk of data(all imported data from excel file )
                        await _context.BulkInsertAsync(list);
                        
                        // wirte imported date to local json file
                        var json = JsonSerializer.Serialize(list);
                        await System.IO.File.WriteAllTextAsync(filePath, json);

                        return Response<List<IruAssignment>>
                        .GetResult(200, "File Imported successfully..", list);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

    }

}




using Microsoft.AspNetCore.Mvc;

namespace CoreAdminWeb.Controllers.Api
{
    /// <summary>
    /// Test Controller để kiểm tra routing
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Test endpoint cơ bản
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                message = "Test Controller hoạt động thành công!", 
                timestamp = DateTime.Now,
                route = "api/test"
            });
        }

        /// <summary>
        /// Test với parameter
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok(new { 
                id = id, 
                message = $"Test với ID: {id}",
                route = $"api/test/{id}"
            });
        }

        /// <summary>
        /// Test medical-data route tương tự DanhSachDoan
        /// </summary>
        /// <param name="maDotKham"></param>
        /// <returns></returns>
        [HttpGet("medical-data")]
        public IActionResult GetMedicalDataTest(string maDotKham)
        {
            return Ok(new { 
                message = "Test medical-data endpoint thành công!",
                maDotKham = maDotKham,
                route = $"api/test/medical-data?maDotKham={maDotKham}",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Test route với path parameter
        /// </summary>
        /// <param name="maDotKham"></param>
        /// <returns></returns>
        [HttpGet("medical-data/{maDotKham}")]
        public IActionResult GetMedicalDataByPath(string maDotKham)
        {
            return Ok(new { 
                message = "Test medical-data với path parameter thành công!",
                maDotKham = maDotKham,
                route = $"api/test/medical-data/{maDotKham}",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Test POST endpoint
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] object data)
        {
            return Ok(new { 
                message = "POST request thành công!",
                receivedData = data,
                route = "api/test"
            });
        }
    }
} 
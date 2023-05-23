using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Interview.Data;
using Microsoft.Data.SqlClient;

namespace Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : ControllerBase
    {
        private readonly DataContext _context;

        public InterviewController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        //public async Task<ActionResult<List<Interview>>> GetInterviews(string type1, string type2, int position, int department, string sortVal, string orderVal)
        public async Task<ActionResult<List<Interview>>> GetInterviews(string type1, string type2, string bandValues, string sortVal, string orderVal)
        {
            string query, usableType;
            string includedBands = "";

            for (int i = 0; i < bandValues.Length; i++)
            {
                if (bandValues[i] == '1')
                {
                    includedBands += (i + 1).ToString() + ", ";
                }
            }

            if (includedBands != "")
            {
                includedBands = includedBands.Remove(includedBands.Length - 2);
            }

            if (type1 == "None" && type2 == "None" || includedBands == "") //if both types are None
            {
                query = "SELECT * FROM Interview WHERE 1 = 2;";
            }

            else if (type1 == "Any" && type2 == "Any")  //if both types are Any
            {
                query = "SELECT * " +
                        "FROM Interview " +
                        "WHERE band IN (" + includedBands + ") " +
                        "ORDER BY " + sortVal + " " + orderVal + ";";
            }

            else if ((type1 == "Any" && type2 == "None") || (type1 == "None" && type2 == "Any"))    //if one type is Any and the other is None
            {
                query = "SELECT * " +
                        "FROM Interview " +
                        "WHERE (type1 is NULL OR type2 is NULL) " +
                        "AND band IN (" + includedBands + ") " +
                        "ORDER BY " + sortVal + " " + orderVal + ";";
            }

            else if ((type1 == "Any" && type2 != "None") || (type1 != "None" && type2 == "Any"))    //if one type is Any and the other is not None
            {
                usableType = (type1 == "Any" ? type2 : type1);
                query = "SELECT * " +
                        "FROM Interview " +
                        "WHERE ((type1 = '" + usableType + "') OR (type2 = '" + usableType + "')) " +
                        "AND band IN (" + includedBands + ") " +
                        "ORDER BY " + sortVal + " " + orderVal + ";";
            }

            else if ((type1 == "None" && type2 != "Any") || (type2 == "None" && type1 != "Any"))  //if one type is None and the other is specific
            {
                usableType = (type1 == "None" ? type2 : type1);
                query = "SELECT * " +
                        "FROM Interview " +
                        "WHERE (type1 = '" + usableType + "' AND type2 IS NULL) " +
                        "AND band IN (" + includedBands + ") " +
                        "ORDER BY " + sortVal + " " + orderVal + ";";
            }

            else  //if both types are specified
            {
                query = "SELECT * " +
                        "FROM Interview " +
                        "WHERE ((type1 = '" + type1 + "' AND type2 = '" + type2 + "') OR (type1 = '" + type2 + "' AND type2 = '" + type1 + "'))" +
                        "AND band IN (" + includedBands + ") " +
                        "ORDER BY " + sortVal + " " + orderVal + ";";
            }

            return Ok(await _context.Interview.FromSqlRaw(query).ToListAsync());
        }

        [HttpGet("name")]
        public async Task<ActionResult<Interview>> SearchInterviewByName(string name)
        {
            var Interview = await _context.Interview.FirstOrDefaultAsync(p => p.name == name);

            if (Interview == null)
            {
                return NotFound();
            }

            return Ok(Interview);
        }
    }
}
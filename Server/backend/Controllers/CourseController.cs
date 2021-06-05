using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Backend.DTO;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdministratorLevel")]
    public class CourseController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public CourseController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        [Authorize(Policy = "ManagerLevel")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            return await _context.Courses.AsQueryable().Cast<CourseDTO>().ToListAsync();
        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return (CourseDTO)course;
        }

        // PUT: api/Course/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(Guid id, CourseDTO courseDTO)
        {
            if (id != courseDTO.CouseID)
            {
                return BadRequest();
            }
            var course = await _context.Courses.FindAsync(id);
            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Course
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(CourseCreationDTO courseDTO)
        {
            if (courseDTO.CourseName is null || courseDTO.ShortCourseName is null || courseDTO.SDUCourseID is null) return BadRequest("Field is null");
            if (await _context.Users.FindAsync(courseDTO.OwnerUsername) == null) return BadRequest("User Does not exist");
            if(!courseDTO.ShortCourseName.Any() || !courseDTO.CourseName.Any() || !courseDTO.SDUCourseID.Any())  return BadRequest("Field Empty");

            var course = new Course()
            {
                CourseName = courseDTO.CourseName,
                ShortCourseName = courseDTO.ShortCourseName,
                CourseID = Guid.NewGuid(),
                SDUCourseID = courseDTO.SDUCourseID,
                UserUsername = courseDTO.OwnerUsername
            };
            var entity = _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetCourse", new { id = course.CourseID }, course);
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
        private string getUsername()
        {
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}

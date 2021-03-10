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

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Administrator")]
    public class CourseController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public CourseController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Courses.ToListAsync();
        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
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
        public async Task<ActionResult<Course>> PostCourse(CourseDTO course)
        {
            if (course.CourseName is null || course.SDUCourseID is null || (await _context.Users.FindAsync(course.OwnerUsername)) != null || !course.CourseName.Any() || !course.SDUCourseID.Any() )  return BadRequest();

            _context.Courses.Add(new Course() 
            { 
                CourseName = course.CourseName,
                CouseID = Guid.NewGuid(),
                SDUCourseID = course.SDUCourseID,
                UserUsername = course.OwnerUsername
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourse", new { id = course.CouseID }, course);
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
            return _context.Courses.Any(e => e.CouseID == id);
        }
        private string getUsername()
        {
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}

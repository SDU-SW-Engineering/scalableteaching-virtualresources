using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.Controllers
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
            var courses = await _context.Courses.Where(course => course.UserUsername == GetUsername()).Cast<CourseDTO>().ToListAsync();

            return courses.Count > 0 ? Ok(courses) : NoContent();
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
            //Validate Request
            if (id != courseDTO.CourseID)
            {
                return BadRequest();
            }
            if (courseDTO.CourseName is null || courseDTO.ShortCourseName is null || courseDTO.SDUCourseID is null) return BadRequest("Field is null");
            if (await _context.Users.FindAsync(courseDTO.OwnerUsername) == null) return BadRequest("User Does not exist");
            if (!courseDTO.ShortCourseName.Any() || !courseDTO.CourseName.Any() || !courseDTO.SDUCourseID.Any()) return BadRequest("Field Empty");

            var CourseValidationResponse = Course.Validate(courseDTO.OwnerUsername, courseDTO.CourseName, courseDTO.ShortCourseName, courseDTO.SDUCourseID);
            if (CourseValidationResponse.Item1 != true)
            {
                return BadRequest(CourseValidationResponse.Item2);
            }

            //Perform Request
            var course = await _context.Courses.FindAsync(id);
            _context.Entry(course).State = EntityState.Modified;

            course.UserUsername = courseDTO.OwnerUsername;
            course.ShortCourseName = courseDTO.ShortCourseName;
            course.SDUCourseID = courseDTO.SDUCourseID;
            course.CourseName = courseDTO.CourseName;

            try
            {
                _context.Courses.Update(course);
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
            if (!courseDTO.ShortCourseName.Any() || !courseDTO.CourseName.Any() || !courseDTO.SDUCourseID.Any()) return BadRequest("Field Empty");

            var CourseValidationResponse = Course.Validate(courseDTO.OwnerUsername, courseDTO.CourseName, courseDTO.ShortCourseName, courseDTO.SDUCourseID);
            if (CourseValidationResponse.Item1 != true)
            {
                return BadRequest(CourseValidationResponse.Item2);
            }
            var course = new Course()
            {
                CourseName = courseDTO.CourseName,
                ShortCourseName = courseDTO.ShortCourseName,
                CourseID = Guid.NewGuid(),
                SDUCourseID = courseDTO.SDUCourseID,
                UserUsername = courseDTO.OwnerUsername
            };
            await _context.Courses.AddAsync(course);
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
        private string GetUsername()
        {
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}

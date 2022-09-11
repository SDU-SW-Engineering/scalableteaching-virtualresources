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

        // GET: api/Course/Administrator
        [HttpGet("/api/administrator/courses")]
        [Authorize(Policy = "AdministratorLevel")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCoursesAdministrator()
        {
            var courses = await _context.Courses.Where(c=> c.Active).Select(course => (CourseDTO)course).ToListAsync();

            return courses.Any() ? Ok(courses) : NoContent();
        }
        // GET: api/Course
        [HttpGet]
        [Authorize(Policy = "")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            var courses = await _context.Courses.Where(course => course.UserUsername == GetUsername() && course.Active).Cast<CourseDTO>().ToListAsync();

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
            
            //If the user enters a username that is the entire email, then shorten the username
            if (courseDTO.OwnerUsername.Contains('@')) courseDTO.OwnerUsername = courseDTO.OwnerUsername.Split('@')[0];
            
            if (courseDTO.CourseName is null || courseDTO.ShortCourseName is null || courseDTO.SDUCourseID is null) return BadRequest("Field is null");
            if (await _context.Users.FindAsync(courseDTO.OwnerUsername) == null) return BadRequest("User Does not exist");
            if (!courseDTO.ShortCourseName.Any() || !courseDTO.CourseName.Any() || !courseDTO.SDUCourseID.Any()) return BadRequest("Field Empty");

            var courseValidationResponse = Course.Validate(courseDTO.OwnerUsername, courseDTO.CourseName, courseDTO.ShortCourseName, courseDTO.SDUCourseID);
            if (courseValidationResponse.Item1 != true)
            {
                return BadRequest(courseValidationResponse.Item2);
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
            //If the user enters a username that is the entire email, then shorten the username
            if (courseDTO.OwnerUsername.Contains('@')) courseDTO.OwnerUsername = courseDTO.OwnerUsername.Split('@')[0];
            
            if (courseDTO.CourseName is null || courseDTO.ShortCourseName is null || courseDTO.SDUCourseID is null) return BadRequest("Field is null");
            if (await _context.Users.FindAsync(courseDTO.OwnerUsername) == null) return BadRequest("User Does not exist");
            if (!courseDTO.ShortCourseName.Any() || !courseDTO.CourseName.Any() || !courseDTO.SDUCourseID.Any()) return BadRequest("Field Empty");

            var courseValidationResponse = Course.Validate(courseDTO.OwnerUsername, courseDTO.CourseName, courseDTO.ShortCourseName, courseDTO.SDUCourseID);
            if (courseValidationResponse.Item1 != true)
            {
                return BadRequest(courseValidationResponse.Item2);
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

            return CreatedAtAction("GetCourse", new { id = course.CourseID }, (CourseDTO)course);
        }

        // DELETE: api/Course/5
        /// <summary>
        /// Schedule deletion of a course and all of its associated machines.
        /// The course is not deleted until all of its associated machines are gone.
        /// </summary>
        /// <param name="id">The id for the course to be deleted</param>
        /// <returns>
        ///     <list type="table">
        ///         <item><term>204</term><description>On successful scheduling for deletion</description></item>
        ///         <item><term>400</term><description>If the course is already scheduled for deletion</description></item>
        ///         <item><term>404</term><description>If the course does not exist</description></item>
        ///     </list>
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            //Getting course
            var course = await _context.Courses.FindAsync(id);
            
            //Validating course ( Existence, Active )
            if (course == null) return NotFound();
            if (!course.Active) return BadRequest("Course scheduled for deletion");
            
            //Disabling course
            course.Active = false;
            
            //Scheduling all machines for deletion
            var machines = await _context.Machines.Where(machine => machine.CourseID == id).ToListAsync();
            var mdr = new List<MachineDeletionRequest>(); //New MachineDeletionRequests to be added to the context in one fell swoop.
            
            //Iterating machines associated with a course to create deletion request associated with each machine
            foreach (var machine in machines)
            {
                //Don't create a request if one already exists
                if (await _context.MachineDeletionRequests.AnyAsync(
                        r => r.MachineID == machine.MachineID)) continue;
                //Create a request and add it to a list.
                mdr.Add(new MachineDeletionRequest()
                    {
                        MachineID = machine.MachineID, //To specify the machine
                        DeletionDate = DateTime.UtcNow, //To ensure swift deletion
                        UserUsername = GetUsername() //For future ability to detect deleter
                    }
                );
                //Setting machine creation status
                machine.MachineCreationStatus = CreationStatus.SHEDULED_FOR_DELETION;
                
            }
            
            //Add all request to the context
            _context.MachineDeletionRequests.AddRange(mdr);
            //Add all machine changes to the context
            _context.Machines.UpdateRange(machines);
            //Add course changes to the context
            _context.Courses.Update(course);
            
            //Saving changes
            await _context.SaveChangesAsync();

            //Returning 204
            return NoContent();
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
        private string GetUsername()
        {
            return HttpContext.User.Claims.First(claim => claim.Type == "username").Value.ToLower();
        }
    }
}

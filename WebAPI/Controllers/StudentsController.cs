using Authentication;
using AutoMapper;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentsController(IMapper mapper,IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("NewStudent")]
        public async Task<IActionResult> NewStudent([FromBody] StudentModel model)
        {
            if (ModelState.IsValid)
            {
                var existingStudent = _unitOfWork.Students.FindByCondition(s=>s.Name==model.Name);

                if (existingStudent.Count()> 0)
                {
                    return BadRequest("Student already registered");
                }

                model.IsActive = false;
                model.IsDeleted = false;
                var student = _mapper.Map<Student>(model);
                _unitOfWork.Students.Add(student);
                var Succeeded= _unitOfWork.Complete();
                if (Succeeded>0)
                {
                    return Ok(model);
                }
                else
                {
                    return BadRequest( "Cannot insert this student");
                }
            }

            return BadRequest("Invalid model");
        }
        [HttpGet]
        [Route("GetAllStudent")]
        [Authorize(Roles ="Admin,Staff")]
        public async Task<IActionResult> GetAllStudent()
        {
            var AllStudents = _unitOfWork.Students.GetAll();
            if (AllStudents.Count()>0)
            {
                return Ok(AllStudents.ToList());
            }
            else
            {
                return Ok("There is no Students To show");
            }

        }

        [HttpPost]
        [Route("AcceptStudent")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> AcceptStudent(int studentId=0,bool Accepted=true)
        {
            if (studentId>0)
            {
                var existingStudent = _unitOfWork.Students.FindByCondition(s => s.StudentId == studentId).FirstOrDefault();

                if (existingStudent==null)
                {
                    return BadRequest("There is no student with requsted details");
                }

                existingStudent.IsActive = true;
                existingStudent.Accepted= Accepted;
                _unitOfWork.Students.Update(existingStudent);
                var Succeeded = _unitOfWork.Complete();
                if (Succeeded > 0)
                {
                    var studentModel = _mapper.Map<StudentModel>(existingStudent);
                    return Ok(studentModel);
                }
                else
                {
                    return BadRequest("Cannot update this student");
                }
            }

            return BadRequest("Invalid model");
        }
    }
}

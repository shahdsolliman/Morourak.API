using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services
{
    public abstract class LicenseProcessingService<TEntity, TApplication>
        where TEntity : class
        where TApplication : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IGenericRepository<CitizenRegistry> _citizenRepo;
        protected readonly IGenericRepository<TEntity> _licenseRepo;
        protected readonly IGenericRepository<Appointment> _examRepo;
        protected readonly IServiceRequestService _serviceRequestService;

        protected LicenseProcessingService(IUnitOfWork unitOfWork, IServiceRequestService serviceRequestService)
        {
            _unitOfWork = unitOfWork;
            _citizenRepo = _unitOfWork.Repository<CitizenRegistry>();
            _licenseRepo = _unitOfWork.Repository<TEntity>();
            _examRepo = _unitOfWork.Repository<Appointment>();
            _serviceRequestService = serviceRequestService;
        }

        #region Helpers

        protected async Task<CitizenRegistry> GetCitizenAsync(string nationalId)
        {
            var citizen = await _citizenRepo.GetAsync(c => c.NationalId == nationalId);
            if (citizen == null) throw new AppEx.ValidationException("??????? ??? ?????.", "CITIZEN_NOT_FOUND");
            return citizen;
        }

        protected async Task<CitizenRegistry> GetCitizenAsync(int id)
        {
            var citizen = await _citizenRepo.GetAsync(c => c.Id == id);
            if (citizen == null) throw new AppEx.ValidationException($"??????? ???????? {id} ??? ?????.", "CITIZEN_NOT_FOUND");
            return citizen;
        }

        protected async Task<Appointment> GetLatestExamAsync(string nationalId)
        {
            var exams = await _examRepo.FindAsync(e => e.CitizenNationalId == nationalId);
            return exams.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
        }

        protected async Task<string> SaveFileAsync(byte[] fileBytes, string folderName)
        {
            var folderPath = Path.Combine("Uploads", folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}.bin";
            var fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(fullPath, fileBytes);
            return fullPath;
        }
        protected async Task CreateServiceRequestAsync(int entityId, ServiceType serviceType, RequestStatus status, string citizenNationalId)
        {
            await _serviceRequestService.CreateAsync(serviceType, entityId, status, citizenNationalId);
        }
        #endregion
    }
}

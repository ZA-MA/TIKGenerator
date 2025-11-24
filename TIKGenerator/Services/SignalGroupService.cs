using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TIKGenerator.Data;
using TIKGenerator.Models;

namespace TIKGenerator.Services
{
    public interface ISignalGroupService
    {
        Task<bool> SaveGroupAsync(SignalGroup group);
        Task<bool> UpdateGroupAsync(SignalGroup group);
        Task<List<SignalGroup>> GetAllGroupsAsync();
        Task DeleteGroupAsync(string id);
    }

    public class SignalGroupService : ISignalGroupService
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly ILogger<SignalGroupService> _logger;

        public SignalGroupService(DbContextOptions<AppDbContext> options, ILogger<SignalGroupService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<bool> SaveGroupAsync(SignalGroup group)
        {
            try
            {
                await using var db = new AppDbContext(_options);
                await db.SignalGroups.AddAsync(group);
                await db.SaveChangesAsync();

                _logger.LogInformation("Группа {GroupId} успешно сохранена.", group.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении группы {GroupId}", group?.Id);
                return false;
            }
        }

        public async Task<bool> UpdateGroupAsync(SignalGroup group)
        {
            try
            {
                await using var db = new AppDbContext(_options);

                var existingGroup = await db.SignalGroups
                    .Include(g => g.Signals)
                    .FirstOrDefaultAsync(g => g.Id == group.Id);

                if (existingGroup == null)
                {
                    _logger.LogWarning("Группа {GroupId} не найдена для обновления.", group.Id);
                    return false;
                }

                existingGroup.Name = group.Name;
                existingGroup.CreatedAt = group.CreatedAt;

                db.Signals.RemoveRange(existingGroup.Signals);
                existingGroup.Signals = group.Signals;

                await db.SaveChangesAsync();

                _logger.LogInformation("Группа {GroupId} успешно обновлена.", group.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении группы {GroupId}", group?.Id);
                return false;
            }
        }

        public async Task<List<SignalGroup>> GetAllGroupsAsync()
        {
            try
            {
                await using var db = new AppDbContext(_options);
                return await db.SignalGroups
                               .Include(g => g.Signals)
                               .OrderByDescending(g => g.CreatedAt)
                               .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех групп");
                return new List<SignalGroup>();
            }
        }

        public async Task DeleteGroupAsync(string id)
        {
            await using var db = new AppDbContext(_options);
            var entity = await db.SignalGroups.FirstOrDefaultAsync(g => g.Id == id);
            if (entity != null)
            {
                db.SignalGroups.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}

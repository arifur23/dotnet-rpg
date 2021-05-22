using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mapper = mapper;

        }
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            Character character = _mapper.Map<Character>(newCharacter);
            character.user = await _context.Users.FirstOrDefaultAsync(c => c.Id == GetUserId());
            await _context.Characters.AddAsync(character);
            await _context.SaveChangesAsync();
            serviceResponse.Data = (_context.Characters.Where(c => c.user.Id == GetUserId()).Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {

                Character character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id && c.user.Id == GetUserId());
                if(character != null)
                {
                    _context.Characters.Remove(character);
                await _context.SaveChangesAsync();
                serviceResponse.Data = (_context.Characters.Where(c => c.user.Id == GetUserId()).Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            List<Character> dbCharacters = await _context.Characters.Where(c => c.user.Id == GetUserId()).ToListAsync();
            serviceResponse.Data = (dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            Character dbCharacter = await _context.Characters
            .Include(c => c.Weapons)
            .Include(c => c.CharacterSkills)
            .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == id && c.user.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _context.Characters.Include(c => c.user).FirstOrDefaultAsync(c => c.Id == updateCharacter.Id);
                Console.WriteLine(character.Id);
                if(character.user.Id == GetUserId())
                {
                    character.Name = updateCharacter.Name;
                    character.Class = updateCharacter.Class;
                    character.Defence = updateCharacter.Defence;
                    character.HitPoints = updateCharacter.HitPoints;
                    character.Intelligence = updateCharacter.Intelligence;
                    character.Strength = updateCharacter.Strength;

                    _context.Characters.Update(character);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
        );
    }
}
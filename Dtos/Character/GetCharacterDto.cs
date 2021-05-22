using System.Collections.Generic;
using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Models;

namespace dotnet_rpg.Dtos.Character
{
    public class GetCharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Arifur";
        public int HitPoints { get; set; } = 100;
        public int Strength { get; set; } = 10;
        public int Defence { get; set; } = 10;
        public int Intelligence { get; set; } =10;
        public RpgClass Class { get; set; } = RpgClass.Knight;
        public GetWeaponDto Weapons { get; set; }
        public List<GetSkillDto> Skills { get; set; }
    }
}
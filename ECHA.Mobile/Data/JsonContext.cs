using System.Text.Json.Serialization;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Helpers;

namespace ECHA.Mobile.Data
{
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(ConteudoResponseDto))]
    [JsonSerializable(typeof(List<ConteudoResponseDto>))]
    [JsonSerializable(typeof(PagedResult<ConteudoResponseDto>))]
    [JsonSerializable(typeof(TopicoForumDto))]
    [JsonSerializable(typeof(List<TopicoForumDto>))]
    [JsonSerializable(typeof(RankingResponseDto))]
    [JsonSerializable(typeof(QuizResponseDto))]
    [JsonSerializable(typeof(List<QuizResponseDto>))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
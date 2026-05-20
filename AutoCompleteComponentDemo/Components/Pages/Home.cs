using System.ComponentModel.DataAnnotations;
using AutoCompleteComponent;

namespace AutoCompleteComponentDemo.Components.Pages;

public partial class Home
{
    public enum EnumExample
    {
        [Display(Name = "Option 1")]
        Option1,

        [Display(Name = "Option 2")]
        Option2,

        [Display(Name = "Option 3")]
        Option3,
    }

    public class SampleData
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public string GetCustomDropDownLabel()
        {
            return $"Value: {Value}, Name: {Name}";
        }

        public string GetCustomSelectedLabel()
        {
            return $"Selected: Value: {Value}, Name: {Name}";
        }
    }

    public List<AutoCompleteOption<EnumExample, EnumExample>> SelectedEnumValues =
    [
        new() { Id = EnumExample.Option1, Data = EnumExample.Option1 },
    ];

    public string[] StringOptions = ["Option 1", "Option 2", "Option 3"];
    public AutoCompleteOption<string, string>? SelectedStringValue = new()
    {
        Id = "Option 1",
        Data = "Option 1",
    };

    public List<AutoCompleteOption<int, SampleData>> SampleDataOptions =
    [
        new()
        {
            Id = 1,
            Data = new() { Value = 1, Name = "Very good air plane" },
            SelectedLabel = "Very good air plane",
        },
        new()
        {
            Id = 2,
            Data = new() { Value = 2, Name = "The vide coders are not really good" },
            SelectedLabel = "The vide coders are not really good",
        },
        new()
        {
            Id = 3,
            Data = new() { Value = 3, Name = "Another example data" },
            SelectedLabel = "Custom Selected Label",
        },
    ];

    public AutoCompleteOption<int, SampleData>? SelectedSampleDataValue { get; set; }

    public async Task<List<AutoCompleteOption<int, SampleData>>> LoadSampleDataOptionsAsync(
        AutoCompleteSearchArgs searchArgs
    )
    {
        await Task.Delay(1000); // Simulate async data fetching

        // Checks if there is something to search, you can change this logic as you need
        if (searchArgs.IsEmpty())
            return [];

        // You can filter your database here
        return
        [
            .. SampleDataOptions.Where(option =>
                option.Data.Name.Contains(searchArgs.SearchTerm, StringComparison.OrdinalIgnoreCase)
            ),
        ];
    }
}

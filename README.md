# Best autocomplete dropdown component for blazor

It supports async and static search with single and multiple options with pagination, you will have a very good level of freedom using it.

[Here is a demo video](https://drive.google.com/file/d/18-nTNsw_jEpIBCbPc7z3tjWCJ3W-9LBb/view)

## How to install?

### Via NuGet Package (Recommended)

```bash
dotnet add package AutoCompleteComponent
```

Or via Package Manager Console:

```
Install-Package AutoCompleteComponent
```

### Setup in your Blazor project

1. Add the following to your `_Imports.razor`:

```razor
@using AutoComplete
```

2. Reference the CSS and JS files in your `App.razor` or `_Layout.cshtml`:

```html
<link
  href="_content/AutoCompleteComponent/auto-complete.css"
  rel="stylesheet"
/>
<script src="_content/AutoCompleteComponent/auto-complete.js"></script>
```

### Manual Installation (Alternative)

- Copy the `AutoComplete` folder to wherever you want in your project since it is together with your other blazor components;
- Copy the `.css` and `.js` files to your `wwwroot` folder (you can use the same structure from here if you want) and reference them in your `Layout`.

## How to use?

### What is the key?

The key of this component is that you now have a very powerfull pattern to design your forms, such a way it is now simple to add autocompletes with single and multiple options using search very efficient because you no longer need to load all options from your database.
All you need to do is to learn about the `AutoCompleteOption<TKey,TData>`.
Basically you need a `TKey` which I recommend to be a struct type(int, short, string) and a `TData` which could also be a struct or whatever you want.
Ex:

```cs
public class MyEntity{
    public int Id {get;set;}
    public string Name {get;set;}
}


public class MyFormModel{
    // here we use the Id as key and data
    public List<AutoCompleteOption<int,int>> MultipleOptionField {get;set;}

    // here we use Id as key and the instance as data
    public AutoCompleteOption<int,MyEntity> SingleOptionField = new (){
        Id = 1,
        SelectedLabel = "Option when selected",
        DropdownLabel = "Option displayed in the dropdown",
        Data = new MyEntity{
            Id = 1,
            Name = "Option 1"
        }
    };
}
```

### Async way:

By using `async` you will be able to query your data from external APIs or your repositories accessing you database directly, you can also enable pagination if you want.
All you need is to have an `async` function that receives a `AutoCompleteSearchArgs` instance and returns `List<AutoCompleteOption<TKey, TData>>`.
The pagination is enabled by default, you need to handle it in the function that loads the options, if you stop returning results the pagination will stop, so keep returning data to keep the user scrolling down.

```cs
<AsyncSingleAutoComplete
    @bind-Value="@Model.Brand"
    OnSearch="@(GetBrandOptions)" />


@code{

    public class MyModel{
        public AutoCompleteOption<short,short> Brand {get;set;}
    }

    MyModel Model = new();

    async Task<List<AutoCompleteOption<short, short>>> GetBrandOptions(AutoCompleteSearchArgs args){
        ... use your own logic to load options based on the args

        return [];
    }
}
```

### Static way:

By using `static` version you only need the options to be preloaded or actually be static, that's it.
PLUS: you have available some extension methods to create options automatically from `enum` or from list of strings for example. The search will occur based on the value displayed to the user thats why you don't need to have a function to search the results.
Ex:

```cs
<StaticMultiAutoComplete
    @bind-Value="@Model.Brands"
    disabled="@IsLoading"
    Options="@AutoCompleteOptions" CloseDropdownOnSelect="false" />

@code{

    List<string> Options = ["Option 1", "Option 2"];

    var AutoCompleteOptions = Options.ToAutoCompleteOptions();

    public class MyModel{
        public List<AutoCompleteOption<string,string>> Brands {get;set;}
    }

    bool IsLoading = false;

    MyModel Model = new();
}
```

### What if you need a very customizable dropdown option or selected option?

Let's suppose you need to display an image in the dropdown options for example.
Well, in this case you can fully customize the rendered option using the fragments.
The context of the selected item may vary depending on the needs of the component whether it is single or multiple choice field, inspect the code to understand a little more.
Ex:

```cs
<StaticSingleOption ...>
    // here you can customize what is rendered when the item is selected
    <SelectedItemTemplate Context="selected">
        @selected.Item.Name
    </SelectedItemTemplate>

    // here you can customize what is rendered in the dropdown
    <DropdownOptionTemplate Context="option">
        <img src="@option.Data.ImageUrl"/>
        <span>@option.Data.Name</span>
    </DropdownOptionTemplate>

</StaticSingleOption>
```

#### Some points

- As my language is PT-BR I wrote the messages for the component in PT-BR but you can customize using some parameters. Ex:

```cs
<AsyncSingleAutoComplete
    Placeholder="Click to search..."
    SearchInputPlaceholder="Type to search..."
    NoResultsMessage="No results found."
    TriggerSearchMessage="Start typing to search"
    LoadingMessage="Loading..."
    LoadingMoreMessage="Loading next page..."
/>
```

- There are also some functional arguments. Ex:

```cs
<AsyncSingleAutoComplete
    CloseDropdownOnSelect="false"
    MinLength="3"
    AutoLoad="true"
    Disabled="false"
    PageSize="20"
/>
```

- By using the `AutoCompleteOption<TKey,TData>` in your models they can be easily cached to json and restored, very usefull for listing pages where you need to store the filters the user used in some url parameter. There is also a good benefit that you can have methods in the TData class if you want and access them very easily when user selects an option.

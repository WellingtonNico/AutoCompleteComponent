# Best autocomplete dropdown component for blazor

It supports async and static search with single and multiple options with pagination, you will have a very good level of freedom using it.

## How to install?

- Copy the `AutoComplete` folder to wherever you want in your project since it is together with your other blazor components;
- Change the namespace according to your needs, you can make a full replace by switching `namespace AutoComplete` to whatever you need;
- Copy the `.css` and `.js` files to your `wwwroot` folder(you can use the same structure from here if you want) and reference them in your `Layout`.

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
    public AutoCompleteOption<int,MyEntity> SingleOptionField {get;set;}
}
```

### Async way:

By using `async` you will be able to query your data from external APIs or your repositories accessing you database directly, you can also enable pagination if you want.
All you need is to have an `async` function that receives a `AutoCompleteSearchArgs` instance and returns `List<AutoCompleteOption<TKey, TData>>`

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

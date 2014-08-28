Code generator that creates observable properties by minimal syntax

## Syntax

```
PropertyCG.exe <folder> <search-pattern>
```

## Language

```
[&]Type[:PropertyName][{EnumValues}] [=][^][!][#][+][$] ['comment']
```

- If Type and PropertyName are equal, PropertyName may be ommitted.
- If EnumValues are specified, an Enum type will also be generated

### Description

Symbol      | Description
------------|-------------------------------
`&`         | Referential property
`=`         | Add a Is{0}Enabled property
`^`         | Add a Is{0}Visible property
`\!`        | Add validation callback
`\#`        | Add property change callback
`+`         | Affects visual model
`$          | Affects results
`'comment'` | Text to use in the summary xml comment ("Gets or sets the \{comment}")

### Dependencies

Change in PropertyA should raise a PropertyChanged event on PropertyB:

```
PropertyA -> PropertyB
```

### Examples

```
double:Coefficient
Length
Length:ActualLength
AnalysisType{Linear,NonLinear} 'type of analysis'
&LoadCase
Length -> ActualLength
```
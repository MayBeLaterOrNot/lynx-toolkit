Objective
---------
* Generate observable properties by minimal syntax

Syntax
---------

{code}[&]Type[:PropertyName][{EnumValues}] [=][^][!][#][+][$] ['comment']{code}

Note: if Type and PropertyName are equal, PropertyName may be ommitted.

Description
---------
| {{&}}  | Referential property |
| {{=}}  | Add a Is{0}Enabled property |
| {{^}}  | Add a Is{0}Visible property |
| {{\!}} | Add validation callback |
| {{\#}} | Add property change callback |
| {{+}}  | Affects visual model |
| {{$}}  | Affects results |
| {{'comment'}} | Text to use in the summary xml comment ("Gets or sets the \{comment}") |

Examples
---------
{code}
double:Height
Length
AnalysisType{Linear,NonLinear} 'type of analysis'
&LoadCase
{code}
;#DropDB Admiral.DataImport.SampleEasyTest

#Application Admiral.DataImport.Sample.Win
#Application Admiral.DataImport.Sample.Web
#IfDef Admiral.DataImport.Sample.Win

*Action About...
 
*Action OK

#EndIf

#IfDef Admiral.DataImport.Sample.Web
!Action Navigation(My Details)
#EndIf

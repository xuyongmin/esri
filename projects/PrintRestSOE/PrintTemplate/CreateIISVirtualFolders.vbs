' Creates\removes IIS Virtual Folders
If WScript.Arguments.Count<2 then 
  WScript.Echo "Wrong arguments."
  WScript.Quit(0)
end if 
sAction = WScript.Arguments(0)
If sAction="Create" then
  If WScript.Arguments.Count<3 then
    WScript.Echo "Wrong arguments."
    WScript.Quit(0)
  End if
  sName = WScript.Arguments(1)
  sPath = WScript.Arguments(2)
  'WScript.Echo sAction & sName & sPath
  On Error Resume Next
  Set fso = CreateObject("Scripting.FileSystemObject")
  sPath = fso.GetAbsolutePathName(sPath)
  If Err.Number<>0 then
    Err.Clear
    WScript.Echo "Can't create '" & sName & "': invalid virtual folder path."
    WScript.Quit(0)
  End if
  Set oIIS = GetObject("IIS://localhost/W3SVC/1/Root")
  If Err.Number<>0 then
    Err.Clear
    WScript.Echo "Error creating '" & sName & "' virtual folder: can't access IIS."
    WScript.Quit(0)
  End if
  Set oDir = oIIS.GetObject("IISWebVirtualDir", sName)
  ' This will return error -2147024893 if it doesn't exist
  If Err.Number=0 then
    WScript.Echo "Can't create '" & sName & "': virtual folder already exists."
    WScript.Quit(0)
  End if
  Err.Clear
  Set oDir = oIIS.Create("IISWebVirtualDir", sName)
  oDir.AccessScript = True
  oDir.Path = sPath
  oDir.SetInfo
  oDir.AppCreate True
  oDir.SetInfo
ElseIf sAction="Remove" then
  sName = WScript.Arguments(1)
  On Error Resume Next
  Set oIIS = GetObject("IIS://localhost/W3SVC/1/Root")
  If Err.Number<>0 then
    Err.Clear
    WScript.Echo "Error removing '" & sName & "' virtual folder: can't access IIS."
    WScript.Quit(0)
  End if
  Set oDir = GetObject("IIS://localhost/W3SVC/1/Root/" & sName)
  If Err.Number<>0 then
    Err.Clear
    WScript.Echo "Error removing '" & sName & "': virtual folder doesn't exist."
    WScript.Quit(0)
  Else
    Err.Clear
    'No error so directory registration exists, we need to remove it
    Set oIIS  = GetObject("IIS://localhost/W3SVC/1")
    Set oRoot = oIIS.GetObject("IIsWebVirtualDir","Root")
    oRoot.Delete "IIsWebVirtualDir", sName
  End If
End If
Module EXE6Module
    'API設定(INI操作用)----------------------------------------------------------------------------
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer

    Public Function WriteStringToINI(ByVal strSection As String, ByVal strEntry As String, ByVal strData As String, Optional ByVal strFile As String = "") As Integer
        '<機能>
        '   INIファイルへ書き込み
        '<戻り値>
        '   APIの戻り値
        '<引数>
        '   ARG1(I)     セクション
        '   ARG2(I)     エントリ
        '   ARG3(I)     データ
        '   ARG4(I)     INIファイル名
        '
        WriteStringToINI = WritePrivateProfileString(strSection, strEntry, strData & Chr(0), strFile)
    End Function

    Public Function GetStringFromINI(ByVal strSection As String, ByVal strEntry As String, ByVal strFile As String, Optional ByVal strDefault As String = "") As String
        '<機能>
        '   INIファイルから読み込み
        '<戻り値>
        '   読み込み結果(="":無し)
        '<引数>
        '   ARG1(I)     セクション
        '   ARG2(I)     エントリ
        '   ARG3(I)     INIファイル名
        '   ARG4(I)     [デフォルトデータ]
        '
        Dim intRet As Integer
        Dim strWork As String
        Dim strResult As String

        strWork = StrDup(1023, " ") & Chr(0)
        intRet = GetPrivateProfileString(strSection, strEntry, strDefault, strWork, 1023, strFile)
        '末尾のChr(0)を取る為の細工
        strResult = Replace(strWork, Chr(0), "")
        strResult = Trim(strResult)

        GetStringFromINI = strResult
    End Function

    Public Function ZeroString(ByVal value As Long, ByVal num As Integer) As String
        ZeroString = Microsoft.VisualBasic.Right("00000000" & Hex(value), num)
    End Function

    Public Function GetAppPath() As String
        Return System.IO.Path.GetDirectoryName( _
            System.Reflection.Assembly.GetExecutingAssembly().Location)
    End Function
End Module

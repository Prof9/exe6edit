Module EXE6Module
    'API�ݒ�(INI����p)----------------------------------------------------------------------------
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer

    Public Function WriteStringToINI(ByVal strSection As String, ByVal strEntry As String, ByVal strData As String, Optional ByVal strFile As String = "") As Integer
        '<�@�\>
        '   INI�t�@�C���֏�������
        '<�߂�l>
        '   API�̖߂�l
        '<����>
        '   ARG1(I)     �Z�N�V����
        '   ARG2(I)     �G���g��
        '   ARG3(I)     �f�[�^
        '   ARG4(I)     INI�t�@�C����
        '
        WriteStringToINI = WritePrivateProfileString(strSection, strEntry, strData & Chr(0), strFile)
    End Function

    Public Function GetStringFromINI(ByVal strSection As String, ByVal strEntry As String, ByVal strFile As String, Optional ByVal strDefault As String = "") As String
        '<�@�\>
        '   INI�t�@�C������ǂݍ���
        '<�߂�l>
        '   �ǂݍ��݌���(="":����)
        '<����>
        '   ARG1(I)     �Z�N�V����
        '   ARG2(I)     �G���g��
        '   ARG3(I)     INI�t�@�C����
        '   ARG4(I)     [�f�t�H���g�f�[�^]
        '
        Dim intRet As Integer
        Dim strWork As String
        Dim strResult As String

        strWork = StrDup(1023, " ") & Chr(0)
        intRet = GetPrivateProfileString(strSection, strEntry, strDefault, strWork, 1023, strFile)
        '������Chr(0)�����ׂ̍׍H
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

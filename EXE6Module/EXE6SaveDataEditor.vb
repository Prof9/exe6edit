
Public Class EXE6SaveDataEditor
    Dim dataList As EXE6DataList = New EXE6DataList()

    Public Sub New(ByVal fileName As String, Optional ByVal Xps As Boolean = False)
        ReadFile(fileName)
        If Xps Then GetXpsBaseAddr()
        _endAddr = &H670F + _baseAddr
        _CheckSumAddr = &H1C6C + _baseAddr
        _MaskValAddr = &H1064 + _baseAddr
        maskValue = ReadWord(_MaskValAddr)
        _VersionInfoAddr = &H1C76 + BaseAddr
        UnMask()
        If ReadByte(_VersionInfoAddr) = &H46 Then
            _VersionInfo = 0
            CheckValue = Checkfalther
        ElseIf ReadByte(_VersionInfoAddr) = &H47 Then
            _VersionInfo = 1
            CheckValue = Checkgrayga
        Else
            _VersionInfo = -1
        End If
    End Sub

    Public Sub GetXpsBaseAddr()
        Dim exe6() As Integer = {&H52, &H4F, &H43, &H4B, &H45, &H58, &H45, &H36}
        For j As Integer = &H26 To &H100
            Dim i As Integer = 0
            While (ReadByte(j + i) = exe6(i))
                i += 1
                If i = 8 Then
                    _baseAddr = &H11C + j
                    Exit For
                End If
            End While
        Next
    End Sub

    Private _baseAddr As Integer = &H100
    Private _endAddr As Integer
    Private _CheckSumAddr As Integer
    Private _MaskValAddr As Integer
    Private _VersionInfoAddr As Integer

    Private _VersionInfo As Integer

    Public ReadOnly Property VersionInfo() As String
        Get
            If _VersionInfo = 0 Then
                Return "VersionFalzar"
            ElseIf _VersionInfo = 1 Then
                Return "VersionGregar"
            Else
                Return "VersionUnknown"
            End If
        End Get
    End Property

    Public ReadOnly Property BaseAddr() As Integer
        Get
            Return _baseAddr
        End Get
    End Property

    Public ReadOnly Property endAddr() As Integer
        Get
            Return _endAddr
        End Get
    End Property

    Public ReadOnly Property CheckSumAddr() As Integer
        Get
            Return _CheckSumAddr
        End Get
    End Property

    Public ReadOnly Property MaskValAddr() As Integer
        Get
            Return _MaskValAddr
        End Get
    End Property

    Private maskValue As Long
    Private buffer As Byte()
    Private CheckValue() As Integer
    Private Checkfalther() As Integer = {&H6F, &H8D, &H81, &H18}
    Private Checkgrayga() As Integer = {&H55, &H43, &H17, &H72}

    Public Property MapCodeValue() As Integer
        Get
            Return ReadHalfWord(&H1B84 + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            WriteHalfWord(&H1B84 + _baseAddr, Value)
        End Set
    End Property

    Public Property ZennyValue() As Integer
        Get
            Return ReadWord(&H1BDC + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 999999 Then Value = 999999
            WriteWord(&H1BDC + _baseAddr, Value)
            WriteWord(&H4FE8 + _baseAddr, Value Xor ReadWord(&H60 + _baseAddr))
        End Set
    End Property

    Public Property BugPieceValue() As Integer
        Get
            Return ReadWord(&H1BE0 + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 9999 Then Value = 9999
            WriteWord(&H1BE0 + _baseAddr, Value)
            WriteWord(&H4FF0 + _baseAddr, Value Xor ReadWord(&H18B8 + _baseAddr))
        End Set
    End Property

#Region " サブチップ "

    Private SubshipIndex As Integer

    Public Property SubChipIndex() As Integer
        Get
            Return SubshipIndex
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 5 Then Value = 5
            SubshipIndex = Value
        End Set
    End Property

    Public Property SubChipValue() As Integer
        Get
            Return SubChipValue(SubshipIndex)
        End Get
        Set(ByVal Value As Integer)
            SubChipValue(SubshipIndex) = Value
        End Set
    End Property

    Public Property SubChipValue(ByVal i As Integer) As Integer
        Get
            Return ReadByte(&H31B4 + i + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 8 Then Value = 8
            WriteByte(&H31B4 + i + _baseAddr, Value)
        End Set
    End Property

    Public Property SubChipExistFlag() As Boolean
        Get
            Return SubChipExistFlag(SubChipIndex)
        End Get
        Set(ByVal Value As Boolean)
            SubChipExistFlag(SubChipIndex) = Value
        End Set
    End Property

    Public Property SubChipExistFlag(ByVal i As Integer) As Boolean
        Get
            If (ReadByte(&H560 + i + _baseAddr) Xor CheckValue(0)) = ReadByte(&H4ACC + i + _baseAddr) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteByte(&H4ACC + i + _baseAddr, ReadByte(&H560 + i + _baseAddr) Xor CheckValue(0))
            Else
                WriteByte(&H4ACC + i + _baseAddr, (ReadByte(&H560 + i + _baseAddr) Xor CheckValue(0) + 1))
            End If
        End Set
    End Property

    Public Property MaxOfSubChipValue() As Integer
        Get
            Return ReadByte(&H31A9 + SubshipIndex + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 4 Then Value = 4
            If Value > 8 Then Value = 8
            WriteByte(&H31A9 + SubshipIndex + _baseAddr, Value)
        End Set
    End Property

#End Region

#Region " 改造カード "

    Public Property RemodelCard(ByVal i As Integer) As Integer
        Get
            Dim val As Integer = ReadByte(&H6620 + i + _baseAddr) And &H7FL
            If val = 0 Then
                Return 1
            ElseIf val > &H75 Then
                Return &H75
            Else
                Return val
            End If
        End Get
        Set(ByVal Value As Integer)
            If Value = 0 Then
                Value = 1
            ElseIf Value > &H75 Then
                Value = &H75
            End If
            WriteByte(&H6620 + i + _baseAddr, Value)
        End Set
    End Property

    Public Property RemodelSwitch(ByVal i As Integer) As Boolean
        Get
            If &H80L = (ReadByte(&H6620 + i + _baseAddr) And &H80L) Then
                Return False
            Else
                Return True
            End If
        End Get
        Set(ByVal Value As Boolean)
            Dim addr As Integer = &H6620 + i + _baseAddr
            If Value = False Then
                WriteByte(addr, ReadByte(addr) Or &H80)
            Else
                WriteByte(addr, ReadByte(addr) And &H7FL)
            End If
        End Set
    End Property

    Public Property RemodelCardCount() As Integer
        Get
            Return ReadByte(&H65F0 + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            WriteByte(&H65F0 + _baseAddr, Value)
        End Set
    End Property

    Public Sub SetRemodelCardLoadFlagALL()
        For i As Integer = 0 To &H74
            WriteByte(&H5048 + i + _baseAddr, ReadByte(&H6C0 + i + _baseAddr) Xor CheckValue(1))
        Next
    End Sub

    Public Sub SetRemodelCardLoadFlag(ByVal i As Integer)
        WriteByte(&H5048 + i + _baseAddr, ReadByte(&H6C0 + i + _baseAddr) Xor CheckValue(1))
    End Sub

    Public Property RemodelCardDisplayExistFlag() As Boolean
        Get
            If &H80L = (ReadByte(&H1CA7 + _baseAddr) And &H80L) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteByte(&H1CA7 + _baseAddr, ReadByte(&H1CA7 + _baseAddr) Or &H80L)
            Else
                WriteByte(&H1CA7 + _baseAddr, ReadByte(&H1CA7 + _baseAddr) And &H7FL)
            End If
        End Set
    End Property

    Public Property RemodelCardCapacity() As Integer
        Get
            Return ReadByte(&H661A + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 255 Then Value = 255
            WriteByte(&H661A + _baseAddr, Value)
        End Set
    End Property

#End Region

#Region " ナビカス "

    Public NaviCusIndex As Integer

    Public Property NaviCusDisplayExistFlag() As Boolean
        Get
            If &H20L = (ReadByte(&H1CA6 + _baseAddr) And &H20L) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteByte(&H1CA6 + _baseAddr, ReadByte(&H1CA6 + _baseAddr) Or &H20L)
            Else
                WriteByte(&H1CA6 + _baseAddr, ReadByte(&H1CA6 + _baseAddr) And &HDFL)
            End If
        End Set
    End Property

    Public Property NaviCusValue() As Integer
        Get
            Return NaviCusValue(NaviCusIndex)
        End Get
        Set(ByVal Value As Integer)
            NaviCusValue(NaviCusIndex) = Value
        End Set
    End Property

    Public Property NaviCusValue(ByVal i As Integer) As Integer
        Get
            Return ReadByte(EXE6DataList.NaviCustAddr(i, 2) + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 9 Then Value = 9
            WriteByte(EXE6DataList.NaviCustAddr(i, 2) + _baseAddr, Value)
        End Set
    End Property

    Public Property NaviCusExistFlag() As Boolean
        Get
            Return NaviCusExistFlag(NaviCusIndex)
        End Get
        Set(ByVal Value As Boolean)
            NaviCusExistFlag(NaviCusIndex) = Value
        End Set
    End Property

    Public Property NaviCusExistFlag(ByVal i As Integer) As Boolean
        Get
            If (ReadByte(EXE6DataList.NaviCustAddr(i, 0) + NaviCusIndex + _baseAddr) Xor CheckValue(0)) _
                = ReadByte(EXE6DataList.NaviCustAddr(i, 1) + NaviCusIndex + _baseAddr) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteByte(EXE6DataList.NaviCustAddr(i, 1) + NaviCusIndex + _baseAddr,
                ReadByte(EXE6DataList.NaviCustAddr(i, 0) + NaviCusIndex + _baseAddr) Xor CheckValue(0))
            Else
                WriteByte(EXE6DataList.NaviCustAddr(i, 1) + NaviCusIndex + _baseAddr,
                (ReadByte(EXE6DataList.NaviCustAddr(i, 0) + NaviCusIndex + _baseAddr) Xor CheckValue(0) + 1))
            End If
        End Set
    End Property

#End Region

    Public Property LibraryFlag(ByVal array(,) As Integer, ByVal i As Integer) As Boolean
        Get
            If array(i, 1) = (ReadByte(array(i, 0) + BaseAddr) And array(i, 1)) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            Dim addr As Integer = array(i, 0) + BaseAddr
            If Value = True Then
                WriteByte(addr, ReadByte(addr) Or array(i, 1))
            Else
                WriteByte(addr, ReadByte(addr) And (Not array(i, 1)))
            End If
            SetLibraryFlag(i, array, Value)
        End Set
    End Property

    Public Sub SetLibraryFlag(ByVal index As Integer, ByVal array(,) As Integer, ByVal val As Boolean)
        Dim offset As Integer = 0
        Select Case (array(index, 1))
            Case &H80
                offset = 0
            Case &H40
                offset = 1
            Case &H20
                offset = 2
            Case &H10
                offset = 3
            Case &H8
                offset = 4
            Case &H4
                offset = 5
            Case &H2
                offset = 6
            Case &H1
                offset = 7
        End Select
        offset = (array(index, 0) - &H204C) * 8 + offset + BaseAddr
        If val = True Then
            WriteByte(&H4BE0 + offset, ReadByte(&H8A0 + offset) Xor CheckValue(2))
        Else
            While (ReadByte(&H4BE0 + offset) = (ReadByte(&H8A0 + offset) Xor CheckValue(2)))
                WriteByte(&H4BE0 + offset, ReadByte(&H4BE0 + offset) Xor CheckValue(2))
            End While
        End If
    End Sub

    Public Property ExtendMemoryValue() As Integer
        Get
            Return ReadByte(&H31A5 + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 2 Then Value = 2
            WriteByte(&H31A5 + _baseAddr, Value)
        End Set
    End Property

    Public Property ExtendMemoryExistFlag() As Boolean
        Get
            If (ReadByte(&H551 + _baseAddr) Xor CheckValue(0)) = ReadByte(&H4ABD + _baseAddr) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteByte(&H4ABD + _baseAddr, ReadByte(&H551 + _baseAddr) Xor CheckValue(0))
            Else
                WriteByte(&H4ABD + _baseAddr, (ReadByte(&H551 + _baseAddr) Xor &HFF))
            End If
        End Set
    End Property

    Public Property HourValue() As Integer
        Get
            Return Int(ReadWord(&H1C1C + _baseAddr) / &H34BC0)
        End Get
        Set(ByVal Value As Integer)
            WriteWord(&H1C1C + _baseAddr, Value * &H34BC0 + MinuteValue * &HE10 + SecondValue * &H3C + MsecondValue)
        End Set
    End Property

    Public Property MinuteValue() As Integer
        Get
            Return Int(ReadWord(&H1C1C + _baseAddr) / &HE10) Mod 60
        End Get
        Set(ByVal Value As Integer)
            WriteWord(&H1C1C + _baseAddr, HourValue * &H34BC0 + Value * &HE10 + SecondValue * &H3C + MsecondValue)
        End Set
    End Property

    Public Property SecondValue() As Integer
        Get
            Return Int(ReadWord(&H1C1C + _baseAddr) / &H3C) Mod 60
        End Get
        Set(ByVal Value As Integer)
            WriteWord(&H1C1C + _baseAddr, HourValue * &H34BC0 + MinuteValue * &HE10 + Value * &H3C + MsecondValue)
        End Set
    End Property

    Public Property MsecondValue() As Integer
        Get
            Return ReadWord(&H1C1C + _baseAddr) Mod 60
        End Get
        Set(ByVal Value As Integer)
            WriteWord(&H1C1C + _baseAddr, HourValue * &H34BC0 + MinuteValue * &HE10 + SecondValue * &H3C + Value)
        End Set
    End Property

    Public Property KeyItemsFlag(ByVal i As Integer) As Boolean
        Get
            If ReadByte(&H3134 + i + _baseAddr) = 0 Then
                Return False
            Else
                Return True
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = False Then
                WriteByte(&H4A4C + i + _baseAddr, ReadByte(&H4E0 + i + _baseAddr) Xor &HFF)
                WriteByte(&H3134 + i + _baseAddr, 0)
            Else
                WriteByte(&H4A4C + i + _baseAddr, ReadByte(&H4E0 + i + _baseAddr) Xor CheckValue(0))
                WriteByte(&H3134 + i + _baseAddr, 1)
            End If
        End Set
    End Property

    Public ChipIndex As Integer
    Public Property ChipValue() As Integer
        Get
            Return ReadByte(EXE6DataList.chipAddrList(ChipIndex) + _baseAddr)
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            If Value > 99 Then Value = 99
            WriteByte(EXE6DataList.chipAddrList(ChipIndex) + _baseAddr, Value)
        End Set
    End Property

    Public Sub SetAllChip(Optional ByVal num As Integer = 99)
        For i As Integer = 0 To dataList.ChipNameList().Length - 1
            WriteByte(EXE6DataList.chipAddrList(i) + _baseAddr, num)
        Next
    End Sub

    Public Sub SetDoubleBeast()
        ResetDoubleBeast()

        'Write name.
        WriteWord(&H1180 + _baseAddr, (4 + 2 + 8) << 8)
        WriteHalfWord(&H1184 + _baseAddr, &H2)
        WriteBytes(&H1186 + _baseAddr, {
            &H48, &H4E, &H32, &H4B, &H82, &H19, &H1B, &HE6
        })

        'Write description.
        WriteWord(&H7D0 + _baseAddr, (4 + 2 + 29) << 8)
        WriteHalfWord(&H7D4 + _baseAddr, &H2)
        WriteBytes(&H7D6 + _baseAddr, {
            &HB7, &HA4, &H9F, &HA7, &HC1, &H14, &H4D, &HAB, &HE9,
            &HB8, &HA2, &HA8, &HA1, &H1C, &H4D, &H16, &H83, &HE9,
            &HBE, &HB9, &HB0, &H48, &H4E, &H32, &HE4, &HB2, &HE4, &HB3, &HE6
        })

        'Write palette.
        WriteHalfWords(&HAF0 + _baseAddr, {
            &H7D7F, &H139F, &H225E, &HD1F, &HB1, &H41D0, &H3D0D, &H1330,
            &H6199, &H77FF, &H4EA8, &H39A8, &H2103, &H5AF9, &H5F30, &HC61
        })
    End Sub

    Public Sub ResetDoubleBeast()
        'Reset name.
        WriteWord(&H1180 + _baseAddr, &H0)
        WriteHalfWord(&H1184 + _baseAddr, &H2)
        WriteBytes(&H1186 + _baseAddr, {
            &H7E, &H7E, &H7E, &H7E, &HE6
        })
        WriteZeroBytes(&H118B + _baseAddr, 18 - 5)

        'Reset description.
        WriteWord(&H7D0 + _baseAddr, &H0)
        WriteHalfWord(&H7D4 + _baseAddr, &H2)
        WriteBytes(&H7D6 + _baseAddr, {
            &H7E, &H7E, &H7E, &H7E, &HE6
        })
        WriteZeroBytes(&H7DB + _baseAddr, 64 - 5)

        'Reset palette.
        WriteZeroBytes(&HAF0 + _baseAddr, 16 * 2)
    End Sub

    Public Sub SetBeast()
        ResetBeast()

        If VersionInfo = "VersionGregar" Then
            'Write Gregar name.
            WriteWord(&H1198 + _baseAddr, (4 + 2 + 5) << 8)
            WriteHalfWord(&H119C + _baseAddr, &H2)
            WriteBytes(&H119E + _baseAddr, {
                &H3F, &H34, &HD, &H3C, &HE6
            })

            'Write Gregar description.
            WriteWord(&H834 + _baseAddr, (4 + 2 + 31) << 8)
            WriteHalfWord(&H838 + _baseAddr, &H2)
            WriteBytes(&H83A + _baseAddr, {
                &HE4, &H55, &HE4, &H43, &HE4, &H4D, &H3F, &H34, &HD, &H3C, &HCE, &HE9,
                &H4E, &H34, &H19, &HE4, &HB2, &HE4, &HB3, &H83, &HE9,
                &HA7, &HDA, &HAC, &HC8, &HC0, &HA2, &HAA, &HA1, &HA7, &HE6
            })

            'Write Gregar palette.
            WriteHalfWords(&HB10 + _baseAddr, {
                &H0, &H77FF, &H479E, &H1F3F, &HA7D, &HD77, &H4F4, &H51,
                &H1089, &H18A3, &H4D5F, &H3787, &H7F90, &H5ACC, &H3609, &H2126
            })

        ElseIf VersionInfo = "VersionFalzar" Then
            'Write Falzar name.
            WriteWord(&H1198 + _baseAddr, (4 + 2 + 6) << 8)
            WriteHalfWord(&H119C + _baseAddr, &H2)
            WriteBytes(&H119E + _baseAddr, {
                &H28, &H55, &H32, &H44, &H82, &HE6
            })

            'Write Falzar description.
            WriteWord(&H834 + _baseAddr, (4 + 2 + 34) << 8)
            WriteHalfWord(&H838 + _baseAddr, &H2)
            WriteBytes(&H83A + _baseAddr, {
                &HE4, &H55, &HE4, &H43, &HE4, &H4D, &H28, &H55, &H32, &H44, &H82, &HCE, &HE9,
                &H1B, &H32, &H1F, &H82, &H46, &HE4, &HB2, &HE4, &HB3, &H83, &HE9,
                &HA7, &HDA, &HAC, &HC8, &HB5, &HA2, &HAB, &HDB, &HA7, &HE6
            })

            'Write Falzar palette.
            WriteHalfWords(&HB10 + _baseAddr, {
                &H0, &H7BDE, &H7774, &H49CC, &H44EB, &H7707, &H6E43, &H5582,
                &H40C1, &H1B3E, &H2659, &H9B4, &H76DD, &H557B, &H2039, &H1805
            })

        End If
    End Sub

    Public Sub ResetBeast()
        'Reset name.
        WriteWord(&H1198 + _baseAddr, &H0)
        WriteHalfWord(&H119C + _baseAddr, &H2)
        WriteBytes(&H119E + _baseAddr, {
            &H7E, &H7E, &H7E, &H7E, &HE6
        })
        WriteZeroBytes(&H11A3 + _baseAddr, 18 - 5)

        'Reset description.
        WriteWord(&H834 + _baseAddr, &H0)
        WriteHalfWord(&H838 + _baseAddr, &H2)
        WriteBytes(&H83A + _baseAddr, {
            &H7E, &H7E, &H7E, &H7E, &HE6
        })
        WriteZeroBytes(&H83F + _baseAddr, 64 - 5)

        'Reset palette.
        WriteZeroBytes(&HB10 + _baseAddr, 16 * 2)
    End Sub

    Public Property CompressCommandFlag() As Boolean
        Get
            If (&HFFFFFF0FL = ReadWord(&H2154 + _baseAddr) And &HFFFFFF0FL) AndAlso
                (&HFFFFFFFFL = ReadWord(&H2158 + _baseAddr) And &HFFFFFFFFL) AndAlso
                (&HFFFFFFFFL = ReadWord(&H215C + _baseAddr) And &HFFFFFFFFL) AndAlso
                (&HFFFFFFFFL = ReadWord(&H2160 + _baseAddr) And &HFFFFFFFFL) AndAlso
                (&HFF00F0FFL = ReadWord(&H2164 + _baseAddr) And &HFF00F0FFL) AndAlso
                (&HF0L = ReadByte(&H2168 + _baseAddr) And &HF0L) Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                WriteWord(&H2154 + _baseAddr, ReadWord(&H2154 + _baseAddr) Or &HFFFFFF0F)
                WriteWord(&H2158 + _baseAddr, ReadWord(&H2158 + _baseAddr) Or &HFFFFFFFF)
                WriteWord(&H215C + _baseAddr, ReadWord(&H215C + _baseAddr) Or &HFFFFFFFF)
                WriteWord(&H2160 + _baseAddr, ReadWord(&H2160 + _baseAddr) Or &HFFFFFFFF)
                WriteWord(&H2164 + _baseAddr, ReadWord(&H2164 + _baseAddr) Or &HFF00F0FF)
                WriteByte(&H2168 + _baseAddr, ReadByte(&H2168 + _baseAddr) Or &HF0)
            Else
                WriteWord(&H2154 + _baseAddr, ReadWord(&H2154 + _baseAddr) And &HF0L)
                WriteWord(&H2158 + _baseAddr, ReadWord(&H2158 + _baseAddr) And &H0L)
                WriteWord(&H215C + _baseAddr, ReadWord(&H215C + _baseAddr) And &H0L)
                WriteWord(&H2160 + _baseAddr, ReadWord(&H2160 + _baseAddr) And &H0L)
                WriteWord(&H2164 + _baseAddr, ReadWord(&H2164 + _baseAddr) And &HFF0F00L)
                WriteByte(&H2168 + _baseAddr, ReadByte(&H2168 + _baseAddr) And &HFL)
            End If
        End Set
    End Property

    Public Sub CompLibrary()
        For i As Integer = &H9A1 To &HAC2
            WriteByte(&H4240 + _baseAddr + i, ReadByte(i) Xor CheckValue(2))
        Next
        For i As Integer = &HACD To &HAD9
            WriteByte(&H4240 + _baseAddr + i, ReadByte(i) Xor CheckValue(2))
        Next
        For i As Integer = &HAE0 To &HAFD
            WriteByte(&H4240 + _baseAddr + i, ReadByte(i) Xor CheckValue(2))
        Next

        WriteWord(&H204C + _baseAddr, &HFFFFFF7F)
        WriteWord(&H2050 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2054 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2058 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H205C + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2060 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2064 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2068 + _baseAddr, &HFFFFFFFF)
        WriteWord(&H206C + _baseAddr, &HFFFFFFFF)
        WriteWord(&H2070 + _baseAddr, &HC0FF07E0)
        WriteWord(&H2074 + _baseAddr, &HFCFFFFFF)
    End Sub

    Public Property FolderValue() As Integer
        Get
            Return ReadByte(BaseAddr + &H1C09)
        End Get
        Set(ByVal Value As Integer)
            If Value < 1 Then Value = 1
            If Value > 3 Then Value = 3
            WriteByte(BaseAddr + &H1C09, Value)
        End Set
    End Property

    ''' <summary>
    ''' ファイルを読み込み
    ''' </summary>
    ''' <param name="fileName">セーブデータのファイルパス</param>
    ''' <remarks>ファイルから読み込みバッファに書き込み</remarks>
    Public Sub ReadFile(ByVal fileName As String)
        '読み込むファイルの名前
        'Dim fileName As String = 
        'ファイルを開く
        Dim fs As New System.IO.FileStream(fileName,
            System.IO.FileMode.Open, System.IO.FileAccess.Read)
        'ファイルを読み込むバイト型配列を作成する
        Dim bs(fs.Length - 1) As Byte
        'ファイルの内容をすべて読み込む
        fs.Read(bs, 0, bs.Length)
        '閉じる
        fs.Close()
        buffer = bs
    End Sub

    Public Sub WriteFile(ByVal fileName As String, Optional ByVal XpsRepairment As Boolean = False)
        UnMask()
        If XpsRepairment Then XpsRepair()
        'ファイルを開く
        Dim fs As New System.IO.FileStream(fileName,
            System.IO.FileMode.Create, System.IO.FileAccess.Write)
        'バイト型配列の内容をすべて書き込む
        fs.Write(buffer, 0, buffer.Length)
        '閉じる
        fs.Close()
        UnMask()
    End Sub

    Public Sub UnMask()
        For i As Integer = _baseAddr To _endAddr
            WriteByte(i, ReadByte(i) Xor maskValue)
        Next
        WriteWord(_MaskValAddr, maskValue)
    End Sub

    Public ReadOnly Property CheckSumValue() As Integer
        Get
            Return ReadWord(_CheckSumAddr)
        End Get
    End Property

    Public Function CalcCheckSum() As Integer
        Dim sum As Integer = 0
        Dim bufSum As Integer = ReadWord(_CheckSumAddr)
        WriteWord(_CheckSumAddr, 0)
        For i As Integer = _baseAddr To _endAddr
            sum += ReadByte(i)
        Next
        sum += CheckValue(3)
        WriteWord(_CheckSumAddr, bufSum)
        Return sum
    End Function

    Public Sub XpsRepair()
        For i As Integer = 0 To &H7FFF
            WriteByte(i + &H8000 + _baseAddr - &H100, ReadByte(_baseAddr + i - &H100))
        Next
    End Sub

    Public Function SetCheckSum() As Integer
        Dim sum As Long = CalcCheckSum()
        WriteWord(_CheckSumAddr, sum)
        Return sum
    End Function

    Public Overrides Function ToString() As String
        Dim sb As New System.IO.StringWriter
        sb.WriteLine(My.Resources.StatusChecksumSaveFile & "0x" & ZeroString(ReadWord(_CheckSumAddr), 8))
        sb.WriteLine(My.Resources.StatusChecksumCalculated & "0x" & ZeroString(CalcCheckSum(), 8))
        sb.WriteLine(My.Resources.StatusMaskValue & "0x" & ZeroString(maskValue, 8))
        sb.WriteLine(My.Resources.StatusVersion & My.Resources.ResourceManager.GetString(VersionInfo))
        Return sb.ToString()
    End Function

    Public Function ReadByte(ByVal addr As Long) As Object
        Return buffer(addr)
    End Function

    Public Function ReadHalfWord(ByVal addr As Long) As Object
        Return buffer(addr) Or (buffer(addr + 1) * &H100L)
    End Function

    Public Function ReadWord(ByVal addr As Long) As Object
        Return ReadHalfWord(addr) Or (ReadHalfWord(addr + 2) << 16)
    End Function

    Public Sub WriteByte(ByVal addr As Integer, ByVal value As Long)
        buffer(addr) = value And &HFFL
    End Sub

    Public Sub WriteHalfWord(ByVal addr As Integer, ByVal value As Long)
        buffer(addr) = value And &HFFL
        buffer(addr + 1) = (value And &HFF00L) >> 8
    End Sub

    Public Sub WriteWord(ByVal addr As Integer, ByVal value As Long)
        buffer(addr) = value And &HFFL
        buffer(addr + 1) = (value And &HFF00L) >> 8
        buffer(addr + 2) = (value And &HFF0000L) >> 16
        buffer(addr + 3) = (value And &HFF000000L) >> 24
    End Sub

    Public Sub WriteBytes(ByVal addr As Integer, ByVal value As Byte())
        For index As Integer = 0 To value.Length - 1
            buffer(addr + index) = value(index)
        Next
    End Sub

    Public Sub WriteHalfWords(ByVal addr As Integer, ByVal value As UShort())
        For index As Integer = 0 To value.Length - 1
            buffer(addr + index * 2) = value(index) And &HFFL
            buffer(addr + index * 2 + 1) = (value(index) And &HFF00L) >> 8
        Next
    End Sub

    Public Sub WriteZeroBytes(ByVal addr As Integer, ByVal count As Integer)
        For index As Integer = 0 To count - 1
            buffer(addr + index) = 0
        Next
    End Sub

    Public Function ReadSignedByte(ByVal addr As Long) As Object
        ReadSignedByte = buffer(addr)
        If (buffer(addr) And &H8000) Then
            ReadSignedByte = -((Not buffer(addr)) + 1)
        End If
    End Function

    Public Function ReadSignedHalfWord(ByVal addr As Long) As Object
        ReadSignedHalfWord = ReadHalfWord(addr)
        If (ReadSignedHalfWord And &H8000) Then
            ReadSignedHalfWord = -((Not ReadSignedHalfWord) + 1)
        End If
    End Function

    Public Function ReadSignedWord(ByVal addr As Long) As Object
        ReadSignedWord = ReadWord(addr)
        If (ReadSignedWord And &H80000000L) Then
            ReadSignedWord = -((Not ReadSignedWord) + 1)
        End If
    End Function

End Class

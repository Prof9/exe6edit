Imports System.Windows.Forms
Public Class FindStringForm
    Private _combobox As ComboBox = Nothing
    Private _listBox As ListBox = Nothing
    Private _listview As ListView = Nothing

    Public Overloads Sub Show(ByVal lb As ListBox)
        Me._listBox = lb
        Me.Show()
    End Sub


    Public Overloads Sub Show(ByVal cb As ComboBox)
        Me._combobox = cb
        Me.Show()
    End Sub

    '「次を検索」がクリックされた時
    Private Sub findButton_Click(ByVal sender As System.Object, _
        ByVal e As System.EventArgs) Handles findButton.Click
        If Not _combobox Is Nothing Then
            If reverse.Checked = False Then
                For i As Integer = _combobox.SelectedIndex + 1 To _combobox.Items.Count - 1
                    If _combobox.Items(i) Like "*" & findString.Text & "*" Then
                        _combobox.BeginUpdate()
                        _combobox.SelectedIndex = _combobox.Items.Count - 1
                        _combobox.SelectedIndex = i
                        _combobox.EndUpdate()
                        Exit For
                    End If
                Next
            Else
                For i As Integer = _combobox.SelectedIndex - 1 To 0 Step -1
                    If _combobox.Items(i) Like "*" & findString.Text & "*" Then
                        _combobox.BeginUpdate()
                        _combobox.SelectedIndex = _combobox.Items.Count - 1
                        _combobox.SelectedIndex = i
                        _combobox.EndUpdate()
                        Exit For
                    End If
                Next
            End If
        ElseIf Not _listBox Is Nothing Then

            If reverse.Checked = False Then
                For i As Integer = _listBox.SelectedIndex + 1 To _listBox.Items.Count - 1
                    If _listBox.Items(i) Like "*" & findString.Text & "*" Then
                        _listBox.BeginUpdate()
                        _listBox.SelectedIndex = _listBox.Items.Count - 1
                        _listBox.SelectedIndex = i
                        _listBox.EndUpdate()
                        Exit For
                    End If
                Next
            Else
                For i As Integer = _listBox.SelectedIndex - 1 To 0 Step -1
                    If _listBox.Items(i) Like "*" & findString.Text & "*" Then
                        _listBox.BeginUpdate()
                        _listBox.SelectedIndex = _listBox.Items.Count - 1
                        _listBox.SelectedIndex = i
                        _listBox.EndUpdate()
                        Exit For
                    End If
                Next
            End If
        End If

    End Sub

    '「閉じる」がクリックされた時
    Private Sub closeButton_Click(ByVal sender As Object, _
        ByVal e As System.EventArgs) Handles closeButton.Click

        Me.Close()
    End Sub
End Class
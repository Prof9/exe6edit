<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FindStringForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FindStringForm))
        Me.closeButton = New System.Windows.Forms.Button()
        Me.findButton = New System.Windows.Forms.Button()
        Me.reverse = New System.Windows.Forms.CheckBox()
        Me.findString = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'closeButton
        '
        Me.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        resources.ApplyResources(Me.closeButton, "closeButton")
        Me.closeButton.Name = "closeButton"
        '
        'findButton
        '
        Me.findButton.DialogResult = System.Windows.Forms.DialogResult.OK
        resources.ApplyResources(Me.findButton, "findButton")
        Me.findButton.Name = "findButton"
        '
        'reverse
        '
        resources.ApplyResources(Me.reverse, "reverse")
        Me.reverse.Name = "reverse"
        '
        'findString
        '
        resources.ApplyResources(Me.findString, "findString")
        Me.findString.Name = "findString"
        '
        'label1
        '
        resources.ApplyResources(Me.label1, "label1")
        Me.label1.Name = "label1"
        '
        'FindStringForm
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.closeButton)
        Me.Controls.Add(Me.findButton)
        Me.Controls.Add(Me.reverse)
        Me.Controls.Add(Me.findString)
        Me.Controls.Add(Me.label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FindStringForm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents closeButton As System.Windows.Forms.Button
    Friend WithEvents findButton As System.Windows.Forms.Button
    Friend WithEvents reverse As System.Windows.Forms.CheckBox
    Friend WithEvents findString As System.Windows.Forms.TextBox
    Friend WithEvents label1 As System.Windows.Forms.Label
End Class

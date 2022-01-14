using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolListRemoverLibrary;

namespace ToolListRemoverUI
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, EventArgs e) => Close();

        private void DeleteToolListsButton_Click(object sender, EventArgs e)
        {
            DeleteToolLists();
            listIdsTextBox.Text = string.Empty;
        }

        private void DeleteToolLists()
        {
            List<string> listsIds = GetListsIds();
            if (listsIds.Count > 0)
            {
                List<string> verifiedListsIds = TDMConnector.VerifyListsIds(listsIds);
                List<string> badListsIds = listsIds.Except(verifiedListsIds).ToList();
                if (badListsIds.Count > 0)
                {
                    string badListIdErrorText = "";
                    foreach (string badListId in badListsIds)
                    {
                        badListIdErrorText += badListId + "\n";
                    }
                    if (MessageBox.Show($"Poniższe listy narzędziowe nie zostały znalezione:\n{badListIdErrorText}Czy chcesz kontynuować?", "Błędne listy narzędziowe!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                if (verifiedListsIds.Count == 0)
                {
                    MessageBox.Show("Brak list narzędziowych do usunięcia", "Brak Danych!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                TDMConnector.DeleteNcPrograms(verifiedListsIds);
                if (!deleteNcFilesOnlyCheckBox.Checked)
                {
                    TDMConnector.DeleteToolLists(verifiedListsIds);
                }
                MessageBox.Show("Listy narzędziowe zostały pomyślnie usunięte", "Sukces!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            MessageBox.Show("Brak list narzędziowych do usunięcia", "Brak Danych!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private List<string> GetListsIds()
        {
            List<string> listsIds = new();
            if (listIdsTextBox.Text == "" || string.IsNullOrEmpty(listIdsTextBox.Text) || string.IsNullOrWhiteSpace(listIdsTextBox.Text))
            {
                return listsIds;
            }
            if (separatedByCommaRadioButton.Checked)
            {
                listsIds = listIdsTextBox.Text.Trim().Split(',').ToList();
            }
            if (separatedByNewLineRadioButton.Checked)
            {
                listsIds = listIdsTextBox.Text.Trim().Split('\n').ToList();
            }
            for (int i = 0; i < listsIds.Count; i++)
            {
                listsIds[i] = listsIds[i].Trim();
            }

            return listsIds;
        }
    }
}

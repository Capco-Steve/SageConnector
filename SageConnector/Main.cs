using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTLib;
using MTLib.Objects;

namespace SageConnector
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
			string sessiontoken = MineralTree.GetSessionToken(SCSettings.Username, SCSettings.Password);

			User user = MineralTree.GetCurrentUser(sessiontoken);

			List<Company> companies = MineralTree.GetCompaniesForCurrentUser(sessiontoken);

			// SHOULD HAVE A VALID USER AND A LIST OF COMPANIES AT THIS POINT!
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Calendar
{
    public partial class Calendar : Form
    {
        public delegate void GoButtonClickHandler(object sender, GoButtonEventArgs e);
        public event GoButtonClickHandler GoButtonClicked;
        private void button4_Click(object sender, EventArgs e)
        {
            if (GoButtonClicked == null) return;
            GoButtonEventArgs args = new GoButtonEventArgs(listView1.SelectedItems[0].SubItems[1].Text);
            GoButtonClicked(this, args);
        }

        public Calendar()
        {
            InitializeComponent();
            InitializeDates();
        }
        private struct DatePlans
        {
            public DateTime date;
            public string details;
            public string location;
        }

        List<DatePlans> datePlans = new List<DatePlans>();
        List<DateTime> boldDates = new List<DateTime>();

        private void InitializeDates()
        {
            try
            {
                using (StreamReader sr = new StreamReader("./dates.txt"))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] splitStrings = line.Split('|');
                        DatePlans curDatePlans = new DatePlans();
                        curDatePlans.date = DateTime.ParseExact(splitStrings[0], "d", null);
                        curDatePlans.details = splitStrings[1];
                        curDatePlans.location = splitStrings[2];
                        datePlans.Add(curDatePlans);
                    }
                    int i = 0;
                    foreach (DatePlans item in datePlans)
                    {
                        if (!boldDates.Contains(item.date))
                            boldDates.Add(item.date);
                        if (item.date == monthCalendar1.TodayDate)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = item.details;
                            lvi.SubItems.Add(item.location);
                            listView1.Items.Add(lvi);
                        }
                    }
                    monthCalendar1.BoldedDates = boldDates.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
        private void AppendDates(DatePlans appendItem)
        {
            using (StreamWriter sw = new StreamWriter("./dates.txt", true))
            {
                sw.WriteLine(appendItem.date.ToString("d") + "|" + appendItem.details.ToString() + "|" + appendItem.location.ToString());
            }
        }
        private void StoreAllDates()
        {
            using (StreamWriter sw = new StreamWriter("./dates.txt"))
            {
                foreach (DatePlans datePlan in datePlans)
                {
                    sw.WriteLine(datePlan.date.ToString("d") + "|" + datePlan.details.ToString() + "|" + datePlan.location.ToString());
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (PlansAdd plansAddFrm = new PlansAdd())
            {
                if(plansAddFrm.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = plansAddFrm.detailsText;
                    lvi.SubItems.Add(plansAddFrm.locationText);
                    listView1.Items.Add(lvi);

                    DatePlans addItem = new DatePlans();

                    addItem.date = monthCalendar1.SelectionStart;
                    addItem.details = lvi.Text;
                    addItem.location = lvi.SubItems[1].Text;
                    datePlans.Add(addItem);
                    if (!boldDates.Contains(monthCalendar1.SelectionStart))
                        boldDates.Add(monthCalendar1.SelectionStart);
                    monthCalendar1.BoldedDates = boldDates.ToArray();
                    AppendDates(addItem);
                }
            }
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            currDate.Text = monthCalendar1.SelectionStart.ToString("dddd, MMM d");
            listView1.Items.Clear();
            foreach (DatePlans item in datePlans)
            {
                if (item.date == monthCalendar1.SelectionStart)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = item.details;
                    lvi.SubItems.Add(item.location);
                    listView1.Items.Add(lvi);
                }
            }

            listView1_ItemSelectionChanged(listView1, 
                                           new ListViewItemSelectionChangedEventArgs(
                                               new ListViewItem(), 0, false));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                DatePlans removeItem = new DatePlans();

                removeItem.date = monthCalendar1.SelectionStart;
                removeItem.details = selectedItem.Text;
                removeItem.location = selectedItem.SubItems[1].Text;
                datePlans.Remove(removeItem);
                listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
                if (listView1.Items.Count == 0)
                    boldDates.Remove(monthCalendar1.SelectionStart);
                monthCalendar1.BoldedDates = boldDates.ToArray();
                StoreAllDates();
            }
            catch(Exception err)
            {
                if (listView1.Items.Count > 0)
                    MessageBox.Show("Please select an activity to remove", "Error");
                else
                    MessageBox.Show("Nothing to remove", "Error");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (PlansAdd plansAddFrm = new PlansAdd())
                {
                    ListViewItem curItem = listView1.SelectedItems[0];
                    DatePlans curDatePlans = new DatePlans();
                    curDatePlans.date = monthCalendar1.SelectionStart;
                    curDatePlans.details = curItem.Text;
                    curDatePlans.location = curItem.SubItems[1].Text;

                    plansAddFrm.detailsText = curItem.Text;
                    plansAddFrm.locationText = curItem.SubItems[1].Text;

                    if (plansAddFrm.ShowDialog() == DialogResult.OK)
                    {
                        curItem.Text = plansAddFrm.detailsText;
                        curItem.SubItems[1].Text = plansAddFrm.locationText;

                        DatePlans replace = new DatePlans();
                        replace.date = curDatePlans.date;
                        replace.details = curItem.Text;
                        replace.location = curItem.SubItems[1].Text;

                        datePlans.Remove(curDatePlans);
                        datePlans.Add(replace);
                        StoreAllDates();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an activity to edit", "Error");
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem curItem = listView1.SelectedItems[0];
            MessageBox.Show("Details: " + curItem.Text + "\n" +
                            "Directory: " + curItem.SubItems[1].Text,
                            "Activity information");
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                button4.Enabled = true;
                button3.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                button4.Enabled = false;
                button3.Enabled = false;
                button2.Enabled = false;
            }
        }
    }
    public class GoButtonEventArgs : EventArgs
    {
        public string Path { get; private set; }
        public GoButtonEventArgs(string path)
        {
            Path = path;
        }
    }
}

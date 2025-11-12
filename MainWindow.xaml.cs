using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TypPostriku
{

    public partial class MainWindow : Window
    {
        private DataTable dataTable = new DataTable();
        public MainWindow()
        {
            InitializeComponent();
            GlobalFunctions.LoadSettings();
            SQLconnection.Password = App.appRuntimeData.AppClientSettings.GetValueOrDefault("sql_connection");
            LoadData();
        }


        private async void LoadData()
        {
            try
            {
                string WhereCommand = "";
                if (txt_filter.Text.Length > 0) { WhereCommand = " WHERE [M3code] LIKE '%" + txt_filter.Text + "%' OR [Nazev] LIKE '%" + txt_filter.Text + "%'"; }

                string connection = SQLconnection.Password;
                SqlConnection cnn = new SqlConnection(connection);
                cnn.Open();
                if (cnn.State == ConnectionState.Open) {
                    TabItemControl.SelectedIndex = 0;
                    SqlDataAdapter mDataAdapter = new SqlDataAdapter(new SqlCommand("SELECT * FROM [KodPostriku] " + WhereCommand + " ORDER BY [M3code],[Kod]", cnn));
                    dataTable.Clear();
                    mDataAdapter.Fill(dataTable);

                    DgListView.ItemsSource = dataTable.DefaultView;
                }

        
                cnn.Close();
            }
            catch(Exception ex) { TabItemControl.SelectedIndex = 2; }


        }


        private async void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connection = SQLconnection.Password;
                SqlConnection cnn = new SqlConnection(connection);
                cnn.Open();
                if (cnn.State == ConnectionState.Open)
                {
                    SQLconnection.Background = new SolidColorBrush(Colors.LightGreen);
                    StatusText.Foreground = new SolidColorBrush(Colors.Green);
                    StatusText.Content = "Připojení proběhlo úspěšně";
                }
                else { 
                    SQLconnection.Background = new SolidColorBrush(Colors.Red);
                    StatusText.Foreground = new SolidColorBrush(Colors.Red);
                    StatusText.Content = "Neplatný Connection String";
                }
                cnn.Close();
               
            }
            catch (Exception ex)
            {
                SQLconnection.Background = new SolidColorBrush(Colors.Red);
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                txt_log.Text = ex.StackTrace;
                StatusText.Content = "Neplatný Connection String";
            }
        }

        private void SaveSetting_Click(object sender, RoutedEventArgs e)
        {
            Exception result = GlobalFunctions.SaveSettings();
            if (result == null)
            {
                StatusText.Foreground = new SolidColorBrush(Colors.Green);
                StatusText.Content = "Uložení proběhlo úspěšně";
            }
            else {
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                StatusText.Content = "Nastavení se nepovedlo uložit";
            }
        }

        private void SQLconnection_PasswordChanged(object sender, RoutedEventArgs e)
        {
            App.appRuntimeData.AppClientSettings.Remove("sql_connection");
            App.appRuntimeData.AppClientSettings.Add("sql_connection", SQLconnection.Password);
        }

        private void btn_clean_Click(object sender, RoutedEventArgs e)
        {
            DeleteRec.IsEnabled = false;
            txt_id.Text = "";
        }

        private void DgListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgListView.SelectedIndex > -1)
            {
                DeleteRec.IsEnabled = true;
                txt_id.Text = ((DataRowView)DgListView.SelectedItem).Row.ItemArray[0].ToString();
                txt_mcode.Text = ((DataRowView)DgListView.SelectedItem).Row.ItemArray[1].ToString();
                txt_code.Text = ((DataRowView)DgListView.SelectedItem).Row.ItemArray[2].ToString();
                txt_name.Text = ((DataRowView)DgListView.SelectedItem).Row.ItemArray[3].ToString();
                txt_description.Text = ((DataRowView)DgListView.SelectedItem).Row.ItemArray[4].ToString();
                chb_active.IsChecked = (bool)((DataRowView)DgListView.SelectedItem).Row.ItemArray[5];

                TabItemControl.SelectedIndex = 1;
            }
            
        }

        private void txt_filter_TextChanged(object sender, TextChangedEventArgs e) => LoadData();

        


        private void SaveRec_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                using (SqlConnection connection = new SqlConnection(SQLconnection.Password))
                {
                    String query = "";
                    if (txt_id.Text.Length == 0) { query = "INSERT INTO [KodPostriku] ([M3code],[Kod],[Nazev],[Popisek],[Aktivni]) VALUES (@m3code,@kod,@nazev, @popisek, @aktivni)"; }
                    else { query = "UPDATE [KodPostriku] SET [M3code]=@m3code, [Kod]=@kod, [Nazev]=@nazev, [Popisek]=@popisek, [Aktivni]=@aktivni, [TimeStamp]=@timestamp WHERE [Id]=@id;"; }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@m3code", txt_mcode.Text);
                        command.Parameters.AddWithValue("@kod", txt_code.Text);
                        command.Parameters.AddWithValue("@nazev", txt_name.Text);
                        command.Parameters.AddWithValue("@popisek", txt_description.Text);
                        command.Parameters.AddWithValue("@aktivni", (bool)chb_active.IsChecked ? 1 : 0);
                        if (txt_id.Text.Length > 0 ) { 
                            command.Parameters.AddWithValue("@timestamp", DateTimeOffset.Now.DateTime); 
                            command.Parameters.AddWithValue("@id", txt_id.Text); 
                        }

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        // Check Error
                        if (result < 0) {
                            StatusText.Foreground = new SolidColorBrush(Colors.Red);
                            StatusText.Content = "Chyba Ukládání Dat";
                        } else {
                            StatusText.Foreground = new SolidColorBrush(Colors.Green);
                            StatusText.Content = "Uložení proběhlo úspěšně";
                            LoadData();


                        }

                    }
                }
            }
            catch (Exception Ex) {
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                StatusText.Content = "Chyba Ukládání Dat";
            }
        }

        private void DeleteRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQLconnection.Password))
                {
                    String query = "DELETE FROM [KodPostriku] WHERE [Id]=@id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", txt_id.Text);
                        
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result < 0) {
                            StatusText.Foreground = new SolidColorBrush(Colors.Red);
                            StatusText.Content = "Chyba Mazání Dat";
                        } else {
                            StatusText.Foreground = new SolidColorBrush(Colors.Green);
                            StatusText.Content = "Smazání proběhlo úspěšně";
                            LoadData();
                        }

                    }
                }

            }
            catch (Exception Ex) { }
        }

        private void txt_mcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt_mcode.Text.Length > 0 && txt_code.Text.Length > 0) { SaveRec.IsEnabled = true; } else { SaveRec.IsEnabled = false; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pizzer_v2
{
    public partial class Form1 : Form
    {
        public string account;
        public int accountId;
        public Form1(string account, int accountId)
        {
            InitializeComponent();
            this.account = account;
            this.accountId = accountId;
            WelcomeLabel.Text = ($"Welcome {account}!");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        Dictionary<string, int> pizzas = new Dictionary<string, int>();

        // string pizzas = "";

        int pizza_count = 0;

        Decimal subtotal = 0;


        private void get_pizza(string my_pizza)

        {
            if (pizza_count >= 20)
            {
                MessageBox.Show("You have selected a maximum of 20 pizzas!");
                return;
            }

            if (pizzas.ContainsKey(my_pizza))
            {
                pizzas[my_pizza] = pizzas[my_pizza] + 1;
            } else
            {
                pizzas.Add(my_pizza, 1);
            }

            //pizzas += my_pizza + ", ";
            pizza_count++;

            using (SqlConnection conn = new SqlConnection())
            {                
                conn.ConnectionString = $@"Data Source={Environment.MachineName}\SQLEXPRESS;Database=pizzas;Trusted_Connection=true";
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM tbl_pizzas where pizza_type = @0", conn);
                command.Parameters.Add(new SqlParameter("0", my_pizza));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SelectedPizzas.Items.Add(string.Format("Type: {0}, Cost: {1,2:C}", reader[1], reader[3]));
                        subtotal += (Decimal)reader[3];
                        OrderTotal.Text = subtotal.ToString("c");
                    }

                }
                conn.Close();
                conn.Dispose();
                
            }
        }


        private void checkout()
        {
            using (SqlConnection conn = new SqlConnection())
            {




                conn.ConnectionString = $@"Data Source={Environment.MachineName}\SQLEXPRESS;Database=pizzas;Trusted_Connection=true";
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM accounts WHERE account_name=@accountName", conn);
                command.Parameters.Add(new SqlParameter("accountName", this.account));


                string FirstName = "";
                string LastName = "";
                string Address = "";
                string Phone = "";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    FirstName = reader.GetString(4);
                    LastName = reader.GetString(5);
                    Address = reader.GetString(6);
                    Phone = reader.GetString(7);
                }

                // These need to be in the order of the columns in your db
                // if your columns go, id, name, address, phone number then put the same names below
                // if they go id, address, name, phone number, then do that
                //                                                                    These bits here
                SqlCommand sqlCommand = new SqlCommand("INSERT INTO orders(customer_id, Name, Address, Number, Total)  OUTPUT INSERTED.order_id  VALUES (@customer_id, @Name, @Address, @Number,@Total)", conn);
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);

                da.SelectCommand.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar)).Value = ($"{FirstName} {LastName}");
                da.SelectCommand.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar)).Value = ($"{Address}");
                da.SelectCommand.Parameters.Add(new SqlParameter("@Number", SqlDbType.NVarChar)).Value = ($"{Phone}");
                da.SelectCommand.Parameters.Add(new SqlParameter("@Total", SqlDbType.NVarChar)).Value = ($"{OrderTotal.Text}"); 
                da.SelectCommand.Parameters.Add(new SqlParameter("@customer_id", SqlDbType.NVarChar)).Value = ($"{accountId}");
                SqlParameter param = new SqlParameter("@order_id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                da.SelectCommand.Parameters.Add(param);

                int orderId = (int) da.SelectCommand.ExecuteScalar();

                
                foreach (var kv in pizzas)
                {
                    string pizzas = kv.Key;
                    int quantity = kv.Value;

                    SqlCommand getPizzaCommand = new SqlCommand("SELECT pizza_id FROM tbl_pizzas WHERE pizza_type = @pizzaName", conn);
                    SqlDataAdapter da2= new SqlDataAdapter(getPizzaCommand);
                    da2.SelectCommand.Parameters.Add(new SqlParameter("@pizzaName", pizzas));

                    int pizzaId = 0;
                    using (SqlDataReader reader = da2.SelectCommand.ExecuteReader())
                    { 
                        if (reader.HasRows)
                        {
                            reader.Read();
                            pizzaId = (int) reader["pizza_id"];
                        }
                    }
                        
                    if (pizzaId == 0)
                    {
                        MessageBox.Show("cannot find pizza stoopid");
                        return;
                    }


                    SqlCommand insetCpmmand = new SqlCommand("INSERT INTO order_items(order_id, pizza_id, quantity) VALUES (@order_id , @pizza_id , @quantity)", conn);
                    SqlDataAdapter da3 = new SqlDataAdapter(insetCpmmand);
                    
                    da3.SelectCommand.Parameters.Add(new SqlParameter("@order_id", orderId));
                    da3.SelectCommand.Parameters.Add(new SqlParameter("@pizza_id", pizzaId));
                    da3.SelectCommand.Parameters.Add(new SqlParameter("@quantity", quantity));
                    da3.SelectCommand.ExecuteNonQuery();
                }
                
                MessageBox.Show("Your Order has been placed!");
            }
        }

        #region Pizza Buttons

        private void Cheese_Click(object sender, EventArgs e)
        {
            get_pizza(Cheese.Text);
        }

        private void BBQ_Click(object sender, EventArgs e)
        {
            get_pizza(BBQ.Text);
        }

        private void Meat_Click(object sender, EventArgs e)
        {
            get_pizza(Meat.Text);

        }

        private void Piri_Click(object sender, EventArgs e)
        {
            get_pizza(Piri.Text);

        }

        private void Hawaii_Click(object sender, EventArgs e)
        {
            get_pizza(Hawaii.Text);

        }

        private void Mediteranian_Click(object sender, EventArgs e)
        {
            get_pizza(Mediteranian.Text);

        }

        private void Mexican_Click(object sender, EventArgs e)
        {
            get_pizza(Mexican.Text);

        }

        private void TheWorks_Click(object sender, EventArgs e)
        {
            get_pizza(TheWorks.Text);

        }
        private void Checkout_Click(object sender, EventArgs e)
        {
            checkout();
            SelectedPizzas.Items.Clear();
            subtotal = 0;
            pizza_count = 0;
            OrderTotal.Text = subtotal.ToString("c");
        }

        private void NewOrder_Click(object sender, EventArgs e)
        {
            SelectedPizzas.Items.Clear();
            subtotal = 0;
            OrderTotal.Text = subtotal.ToString("c");
            pizza_count = 0;
        }

        #endregion

        #region Labels
        
        private void OrderTotal_Click(object sender, EventArgs e)
        {

        }

        private void WelcomeLabel_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region List Boxes
        private void SelectedPizzas_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        #endregion

        
    }
}

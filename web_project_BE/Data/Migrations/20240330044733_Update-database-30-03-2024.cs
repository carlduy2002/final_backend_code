using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojectBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatedatabase30032024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    categoryid = table.Column<int>(name: "category_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryname = table.Column<string>(name: "category_name", type: "varchar(255)", nullable: false),
                    categorydescription = table.Column<string>(name: "category_description", type: "varchar(255)", nullable: true),
                    categorystatus = table.Column<string>(name: "category_status", type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.categoryid);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    roleid = table.Column<int>(name: "role_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rolename = table.Column<string>(name: "role_name", type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.roleid);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    sizeid = table.Column<int>(name: "size_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sizenumber = table.Column<int>(name: "size_number", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sizes", x => x.sizeid);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    supplierid = table.Column<int>(name: "supplier_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suppliername = table.Column<string>(name: "supplier_name", type: "varchar(255)", nullable: false),
                    supplieremail = table.Column<string>(name: "supplier_email", type: "varchar(255)", nullable: false),
                    supplieraddress = table.Column<string>(name: "supplier_address", type: "varchar(255)", nullable: false),
                    supplierphone = table.Column<string>(name: "supplier_phone", type: "varchar(10)", nullable: false),
                    supplierstatus = table.Column<string>(name: "supplier_status", type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.supplierid);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    accountid = table.Column<int>(name: "account_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    accountusername = table.Column<string>(name: "account_username", type: "varchar(20)", maxLength: 20, nullable: false),
                    accountemail = table.Column<string>(name: "account_email", type: "varchar(255)", nullable: false),
                    accountpassword = table.Column<string>(name: "account_password", type: "varchar(255)", nullable: false),
                    accountconfirmpassword = table.Column<string>(name: "account_confirm_password", type: "varchar(255)", nullable: false),
                    accountphone = table.Column<string>(name: "account_phone", type: "varchar(10)", nullable: false),
                    accountaddress = table.Column<string>(name: "account_address", type: "varchar(255)", nullable: true),
                    accountbirthday = table.Column<DateTime>(name: "account_birthday", type: "datetime2", nullable: false),
                    accountgender = table.Column<string>(name: "account_gender", type: "varchar(20)", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refeshtoken = table.Column<string>(name: "refesh_token", type: "nvarchar(max)", nullable: true),
                    refeshtokenexprytime = table.Column<DateTime>(name: "refesh_token_exprytime", type: "datetime2", nullable: false),
                    resetpasswordtoken = table.Column<string>(name: "reset_password_token", type: "nvarchar(max)", nullable: true),
                    resetpasswordexprytime = table.Column<DateTime>(name: "reset_password_exprytime", type: "datetime2", nullable: false),
                    accountstatus = table.Column<int>(name: "account_status", type: "int", nullable: false),
                    accountavatar = table.Column<string>(name: "account_avatar", type: "varchar(255)", nullable: true),
                    accountroleid = table.Column<int>(name: "account_role_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.accountid);
                    table.ForeignKey(
                        name: "FK_Accounts_Roles_account_role_id",
                        column: x => x.accountroleid,
                        principalTable: "Roles",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    productid = table.Column<int>(name: "product_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productname = table.Column<string>(name: "product_name", type: "varchar(255)", nullable: false),
                    productquantitystock = table.Column<int>(name: "product_quantity_stock", type: "int", nullable: false),
                    productoriginalprice = table.Column<double>(name: "product_original_price", type: "float", nullable: false),
                    productsellprice = table.Column<double>(name: "product_sell_price", type: "float", nullable: false),
                    productdescription = table.Column<string>(name: "product_description", type: "varchar(255)", nullable: true),
                    productimage = table.Column<string>(name: "product_image", type: "varchar(255)", nullable: true),
                    productimportdate = table.Column<DateTime>(name: "product_import_date", type: "datetime2", nullable: true),
                    productstatus = table.Column<string>(name: "product_status", type: "varchar(15)", nullable: false),
                    psizeid = table.Column<int>(name: "p_size_id", type: "int", nullable: false),
                    pcategoryid = table.Column<int>(name: "p_category_id", type: "int", nullable: false),
                    psupplierid = table.Column<int>(name: "p_supplier_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.productid);
                    table.ForeignKey(
                        name: "FK_Products_Categories_p_category_id",
                        column: x => x.pcategoryid,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Sizes_p_size_id",
                        column: x => x.psizeid,
                        principalTable: "Sizes",
                        principalColumn: "size_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_p_supplier_id",
                        column: x => x.psupplierid,
                        principalTable: "Suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    cartid = table.Column<int>(name: "cart_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    accountid = table.Column<int>(name: "account_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.cartid);
                    table.ForeignKey(
                        name: "FK_Carts_Accounts_account_id",
                        column: x => x.accountid,
                        principalTable: "Accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    orderid = table.Column<int>(name: "order_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderdate = table.Column<DateTime>(name: "order_date", type: "datetime2", nullable: false),
                    deliverydate = table.Column<DateTime>(name: "delivery_date", type: "datetime2", nullable: true),
                    orderaddress = table.Column<string>(name: "order_address", type: "varchar(255)", nullable: false),
                    orderphone = table.Column<string>(name: "order_phone", type: "varchar(10)", nullable: false),
                    orderquantity = table.Column<int>(name: "order_quantity", type: "int", nullable: false),
                    ordernote = table.Column<string>(name: "order_note", type: "varchar(255)", nullable: true),
                    orderstatus = table.Column<string>(name: "order_status", type: "varchar(15)", nullable: false),
                    orderpayment = table.Column<string>(name: "order_payment", type: "varchar(255)", nullable: false),
                    oaccountid = table.Column<int>(name: "o_account_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.orderid);
                    table.ForeignKey(
                        name: "FK_Orders_Accounts_o_account_id",
                        column: x => x.oaccountid,
                        principalTable: "Accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    imageid = table.Column<int>(name: "image_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imageuri = table.Column<string>(name: "image_uri", type: "varchar(255)", nullable: false),
                    iproductid = table.Column<int>(name: "i_product_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.imageid);
                    table.ForeignKey(
                        name: "FK_Images_Products_i_product_id",
                        column: x => x.iproductid,
                        principalTable: "Products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cart_Details",
                columns: table => new
                {
                    cdid = table.Column<int>(name: "cd_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cdquantity = table.Column<int>(name: "cd_quantity", type: "int", nullable: false),
                    cdcartid = table.Column<int>(name: "cd_cart_id", type: "int", nullable: false),
                    cdproductid = table.Column<int>(name: "cd_product_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart_Details", x => x.cdid);
                    table.ForeignKey(
                        name: "FK_Cart_Details_Carts_cd_cart_id",
                        column: x => x.cdcartid,
                        principalTable: "Carts",
                        principalColumn: "cart_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cart_Details_Products_cd_product_id",
                        column: x => x.cdproductid,
                        principalTable: "Products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order_Details",
                columns: table => new
                {
                    odid = table.Column<int>(name: "od_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    odquantity = table.Column<int>(name: "od_quantity", type: "int", nullable: false),
                    odorderid = table.Column<int>(name: "od_order_id", type: "int", nullable: false),
                    odproductid = table.Column<int>(name: "od_product_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Details", x => x.odid);
                    table.ForeignKey(
                        name: "FK_Order_Details_Orders_od_order_id",
                        column: x => x.odorderid,
                        principalTable: "Orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Details_Products_od_product_id",
                        column: x => x.odproductid,
                        principalTable: "Products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_account_role_id",
                table: "Accounts",
                column: "account_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_Details_cd_cart_id",
                table: "Cart_Details",
                column: "cd_cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_Details_cd_product_id",
                table: "Cart_Details",
                column: "cd_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_account_id",
                table: "Carts",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_i_product_id",
                table: "Images",
                column: "i_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_od_order_id",
                table: "Order_Details",
                column: "od_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_od_product_id",
                table: "Order_Details",
                column: "od_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_o_account_id",
                table: "Orders",
                column: "o_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_p_category_id",
                table: "Products",
                column: "p_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_p_size_id",
                table: "Products",
                column: "p_size_id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_p_supplier_id",
                table: "Products",
                column: "p_supplier_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cart_Details");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Order_Details");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

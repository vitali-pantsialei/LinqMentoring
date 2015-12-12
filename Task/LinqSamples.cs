// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("My solutions")]
		[Title("Agregate - Task 001")]
		[Description("Find all clients with summary order amount more than X")]
		public void Linq001()
		{
			const decimal x = 20000;
			var clients =
				from cust in dataSource.Customers
				where cust.Orders.Sum(t => t.Total) > x
				select
					new
					{
						CompanyName = cust.CompanyName,
						OrderTotal = cust.Orders.Sum(t => t.Total)
					};

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Grouping - Task 002g")]
		[Description("Find a list of suppliers for every client with the same city and country")]
		public void Linq002g()
		{
			var clients =
				from cust in dataSource.Customers
				from sup in dataSource.Suppliers
				where cust.City == sup.City && cust.Country == sup.Country
				group sup.SupplierName by cust.CompanyName
					into cby
					select new { Client = cby.Key, Suppliers = cby };

			foreach (var p in clients)
			{
				ObjectDumper.Write(p.Client);
				foreach (var supplier in p.Suppliers)
				{
					ObjectDumper.Write("	" + supplier);
				}
			}
		}

		[Category("My solutions")]
		[Title("Projection - Task 002ng")]
		[Description("Find a list of suppliers for every client with the same city and country")]
		public void Linq002ng()
		{
			var clients =
				from cust in dataSource.Customers
				from sup in dataSource.Suppliers
				where cust.City == sup.City && cust.Country == sup.Country
				select
					new
					{
						CompanyName = cust.CompanyName,
						SupplierName = sup.SupplierName,
						Country = cust.Country,
						City = cust.City
					};

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Quantifiers - Task 003")]
		[Description("Find all clients which have any order with total more than X")]
		public void Linq003()
		{
			const decimal x = 5000;
			var clients =
				from cust in dataSource.Customers
				where cust.Orders.Any(t => t.Total > x)
				select
					new
					{
						CompanyName = cust.CompanyName
					};

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Agregate - Task 004")]
		[Description("Display all clients with their first order date")]
		public void Linq004()
		{
			var clients =
				from cust in dataSource.Customers
				select
					new
					{
						CompanyName = cust.CompanyName,
						ClientFrom = cust.Orders.Count() > 0 ? cust.Orders.Min(t => t.OrderDate).ToString() : "No records"
					};

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Ordering - Task 005")]
		[Description("Display all clients with their first order date by descending")]
		public void Linq005()
		{
			var clients =
				from cust in dataSource.Customers
				select
					new
					{
						CompanyName = cust.CompanyName,
						ClientFrom = cust.Orders.Count() > 0 ? cust.Orders.Min(t => t.OrderDate) : new DateTime(1900),
						TotalOrder = cust.Orders.Sum(t => t.Total)
					}
					into custTemp
					orderby custTemp.ClientFrom, custTemp.TotalOrder descending, custTemp.CompanyName
					select custTemp;

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Quantifiers - Task 006")]
		[Description("Display all clients who have not digital postal code or don't have region or don't have operators phone code")]
		public void Linq006()
		{
			var clients =
				from cust in dataSource.Customers
				where cust.PostalCode == null || !cust.PostalCode.All(c => Char.IsDigit(c)) || String.IsNullOrEmpty(cust.Region) || cust.Phone.Trim()[0] != '('
				select cust;

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Grouping - Task 007")]
		[Description("Group all products by category then by units in stock or not then sort by price")]
		public void Linq007()
		{
			var products =
				from prod in dataSource.Products
				group prod by prod.Category
					into prodby
					select
						new
						{
							Category = prodby.Key,
							InStock = from p in prodby
									  group p by p.UnitsInStock > 0
										  into unitby
										  select
										  new
										  {
											  InStock = unitby.Key,
											  ProductDetails = from u in unitby
															   orderby u.UnitPrice
															   select u
										  }
						};

			foreach (var p in products)
			{
				ObjectDumper.Write(p.Category);
				foreach (var p1 in p.InStock)
				{
					ObjectDumper.Write("	In stock: " + p1.InStock);
					foreach (var p2 in p1.ProductDetails)
					{
						ObjectDumper.Write(p2);
					}
				}
			}
		}

		[Category("My solutions")]
		[Title("Grouping - Task 008")]
		[Description("Group all products low price, middle price, big price")]
		public void Linq008()
		{
			decimal middleStep = 50, bigStep = 100;
			var bigPrice =
				from prod in dataSource.Products
				group prod by prod.UnitPrice > bigStep
					into prodby
					select
						new
						{
							IsBig = prodby.Key,
							ProdDetails = prodby
						};
			var middlePrice =
				from prod in dataSource.Products
				group prod by prod.UnitPrice <= bigStep && prod.UnitPrice > middleStep
					into prodby
					select
						new
						{
							IsMiddle = prodby.Key,
							ProdDetails = prodby
						};
			var smallPrice =
				from prod in dataSource.Products
				group prod by prod.UnitPrice <= middleStep
					into prodby
					select
						new
						{
							IsSmall = prodby.Key,
							ProdDetails = prodby
						};
			ObjectDumper.Write("Small price");
			foreach (var p in smallPrice)
			{
				if (p.IsSmall)
					ObjectDumper.Write(p.ProdDetails);
			}
			ObjectDumper.Write("Middle price");
			foreach (var p in middlePrice)
			{
				if (p.IsMiddle)
					ObjectDumper.Write(p.ProdDetails);
			}
			ObjectDumper.Write("Big price");
			foreach (var p in bigPrice)
			{
				if (p.IsBig)
					ObjectDumper.Write(p.ProdDetails);
			}
		}

		[Category("My solutions")]
		[Title("Grouping - Task 009")]
		[Description("Get avrg orders count by city")]
		public void Linq009()
		{
			var clients =
				from cust in dataSource.Customers
				group cust by cust.City
					into custby
					select new
					{
						City = custby.Key,
						AvrgCount = custby.Average(x => x.Orders.Length),
						AvrgTotal = (from c in custby
									 select
									 new
									 {
										 Count = c.Orders.Length > 0 ? c.Orders.Average(x => x.Total) : 0
									 }).Average(x => x.Count)
					};

			foreach (var p in clients)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("My solutions")]
		[Title("Grouping - Task 010")]
		[Description("Get avrg orders statistic by monthes, years, years and monthes")]
		public void Linq010()
		{
			var monthStat =
				from cust in dataSource.Customers.SelectMany(x => x.Orders)
				group cust by cust.OrderDate.Month
				into monthBy
				select new
				{
					Month = monthBy.Key,
					Avrg = monthBy.Average(x => x.Total)
				};

			var yearStat =
				from cust in dataSource.Customers.SelectMany(x => x.Orders)
				group cust by cust.OrderDate.Year
				into yearBy
				select new
				{
					Year = yearBy.Key,
					Avrg = yearBy.Average(x => x.Total)
				};

			var yearMonthStat =
				from cust in dataSource.Customers.SelectMany(x => x.Orders)
				group cust by new {
					cust.OrderDate.Year,
					cust.OrderDate.Month
				} into yearBy
				select new
				{
					Year = yearBy.Key.Year,
					Month = yearBy.Key.Month,
					Avrg = yearBy.Average(x => x.Total)
				};

			foreach (var p in yearStat)
			{
				ObjectDumper.Write(p);
			}

			ObjectDumper.Write("------------------------");

			foreach (var p in monthStat)
			{
				ObjectDumper.Write(p);
			}

			ObjectDumper.Write("------------------------");

			foreach (var p in yearMonthStat)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

	}
}

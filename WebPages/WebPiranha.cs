﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

using Piranha.Models;

namespace Piranha.WebPages
{
	public static class WebPiranha
	{
		/// <summary>
		/// Initializes the webb app.
		/// </summary>
		public static void Init() {
			// Register virtual path provider for the manager area
			HostingEnvironment.RegisterVirtualPathProvider(new Piranha.Web.ResourcePathProvider()) ;

			AreaRegistration.RegisterAllAreas() ;
			RegisterGlobalFilters(GlobalFilters.Filters) ;
			RegisterRoutes(RouteTable.Routes) ;
			RegisterBinders() ;
		}

		/// <summary>
		/// Initializes the manager app.
		/// </summary>
		/// <param name="context"></param>
		public static void InitManager(AreaRegistrationContext context) {
			context.MapRoute(
				"Manager",
				"Manager/{controller}/{action}/{id}",
				new { controller = "Page", action = "Index", id = UrlParameter.Optional }
			) ;
		}

		/// <summary>
		/// Handles the URL Rewriting for the application
		/// </summary>
		/// <param name="context">Http context</param>
		public static void BeginRequest(HttpContext context) {
			string path = context.Request.Path.Substring(context.Request.ApplicationPath.Length > 1 ? 
				context.Request.ApplicationPath.Length : 0) ;

			// If this is a call to "hem" then URL rewrite
			if (path.StartsWith("/hem/")) {
				Permalink perm = Permalink.GetByName(path.Substring(5)) ;

				if (perm != null) {
					if (perm.Type == Permalink.PermalinkType.PAGE) {
						Page page = Page.GetSingle(perm.ParentId) ;

						if (!String.IsNullOrEmpty(page.Controller)) {
							context.RewritePath("~/templates/" + page.Controller + "/" + perm.Name, false) ;
						} else {
							context.RewritePath("~/page/" + perm.Name) ;
						}
					} else {
						context.RewritePath("~/post/" + perm.Name) ;
					}
				} else {
					string str = path.Substring(5).ToLower() ;
					if (str == "perm") {
						//
						// TODO: Generate RSS feed for all posts
						//
					}
				}
			} else if (path.StartsWith("/media/")) {
				//
				// Media content
				//
				Content content = Content.GetSingle(new Guid(path.Substring(7))) ;

				if (content != null)
					content.GetMedia(context.Response) ;
			} else if (path.StartsWith("/thumb/")) {
				//
				// Thumbnail content
				//
				string[] param = path.Substring(7).Split(new char[] { '/' }) ;
				Content content = Content.GetSingle(new Guid(param[0])) ;

				if (content != null) {
					if (param.Length == 1)
						content.GetThumbnail(context.Response) ;
					else content.GetThumbnail(context.Response, Convert.ToInt32(param[1])) ;
				}
			} else if (path.StartsWith("/upload/")) {
				//
				// Uploaded content
				//
				string [] param = path.Substring(8).Split(new char[] { '/' }) ;
				Upload upload = Upload.GetSingle(new Guid(param[0])) ;

				if (upload != null)
					upload.GetFile(context.Response) ;
			} else if (path == "/") {
				//
				// Rewrite to current startpage
				//
				Page page = Page.GetStartpage() ;

				if (!String.IsNullOrEmpty(page.Controller))
					context.RewritePath("~/templates/" + page.Controller, false) ;
				else context.RewritePath("~/page") ;
			}
		}

		#region Private methods
		/// <summary>
		/// Registers all routes.
		/// </summary>
		/// <param name="routes">The current route collection</param>
		private static void RegisterRoutes(RouteCollection routes) {
			/*routes.MapRoute("Manager",
				"Manager.aspx/{controller}/{action}/{id}",
				new { controller = "Page", action = "Index", id = UrlParameter.Optional }) ;*/
		}

		/// <summary>
		/// Registers all global filters.
		/// </summary>
		/// <param name="filters">The current filter collection</param>
		private static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		}

		/// <summary>
		/// Registers all custom binders.
		/// </summary>
		private static void RegisterBinders() {
			ModelBinders.Binders.Add(typeof(Piranha.Models.Manager.PageModels.EditModel), 
				new Piranha.Models.Manager.PageModels.EditModel.Binder()) ;
			ModelBinders.Binders.Add(typeof(Piranha.Models.Manager.PostModels.EditModel), 
				new Piranha.Models.Manager.PostModels.EditModel.Binder()) ;
			ModelBinders.Binders.Add(typeof(Piranha.Models.Manager.TemplateModels.PageEditModel),
				new Piranha.Models.Manager.TemplateModels.PageEditModel.Binder()) ;
		}
		#endregion
	}
}

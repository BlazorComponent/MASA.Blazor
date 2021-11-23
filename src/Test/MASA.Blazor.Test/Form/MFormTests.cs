﻿using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor.Test.Form
{
    [TestClass]
    public class MFormTests:TestBase
    {
        [TestMethod]
        public void RenderWithChildContent()
        {
            // Arrange & Act
            var cut = RenderComponent<MForm>(props =>
            {
                props.Add(form => form.ChildContent,Counter=> "<span>Hello world</span>");
            });
            var contentDiv = cut.Find(".m-form");

            // Assert
            contentDiv.Children.MarkupMatches("<span>Hello world</span>");
        }
    }
}

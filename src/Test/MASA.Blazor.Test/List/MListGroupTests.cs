﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using BlazorComponent;
using Moq;

namespace MASA.Blazor.Test.List
{
    [TestClass]
    public class MListGroupTests:TestBase
    {
        [TestMethod]
        public void RendeListGroupWithDisabled()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.Disabled, true);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasDisabledClass = classes.Contains("m-list-group--disabled");

            // Assert
            Assert.IsTrue(hasDisabledClass);
        }

        [TestMethod]
        public void RendeListGroupNoWithDisabled()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.Disabled, false);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasDisabledClass = classes.Contains("m-list-group");

            // Assert
            Assert.IsTrue(hasDisabledClass);
        }

        [TestMethod]
        public void RendeListGroupWithNoAction()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.NoAction, true);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasNoActionClass = classes.Contains("m-list-group--no-action");

            // Assert
            Assert.IsTrue(hasNoActionClass);
        }

        [TestMethod]
        public void RendeListGroupNoWithNoAction()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.NoAction, false);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasNoActionClass = classes.Contains("m-list-group");

            // Assert
            Assert.IsTrue(hasNoActionClass);
        }

        [TestMethod]
        public void RendeListGroupWithSubGroup()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.SubGroup, true);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasSubGroupClass = classes.Contains("m-list-group--sub-group");

            // Assert
            Assert.IsTrue(hasSubGroupClass);
        }

        [TestMethod]
        public void RendeListGroupNoWithSubGroup()
        {
            //Act
            var cut = RenderComponent<MListGroup>(props =>
            {
                props.Add(listgroup => listgroup.SubGroup, false);
            });
            var classes = cut.Instance.CssProvider.GetClass();
            var hasSubGroupClass = classes.Contains("m-list-group");

            // Assert
            Assert.IsTrue(hasSubGroupClass);
        }
    }
}

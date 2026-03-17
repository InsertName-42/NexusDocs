using Microsoft.AspNetCore.Mvc;
using NexusDocs.Controllers;
using NexusDocs.Models;
using NexusDocs.Tests.Fakes;
using Xunit;

namespace NexusDocs.Tests
{
    public class PublicPageTests
    {
        [Fact]
        public async Task DisplayInvalidSlug()
        {
            //Arrange
            var fakeRepo = new FakePageRepository();
            var controller = new PublicPageController(fakeRepo, null);

            //Act
            var result = await controller.Display("greg", "wrong-slug");

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DisplayTemplateFile()
        {
            //Arrange
            var fakeRepo = new FakePageRepository();

            //Create the user
            var testUser = new AppUser
            {
                Id = "user-123",
                UserKey = "greg",
                UserName = "greg@example.com"
            };

            fakeRepo.Pages.Add(new PageEntity
            {
                PageEntityId = 1,
                Slug = "my-gifts",
                PageTitle = "Gifts",
                SiteId = 10,
                Site = new SiteEntity
                {
                    SiteEntityId = 10,
                    UserId = testUser.Id,
                    User = testUser,
                    SiteTitle = "Test"
                },
                Template = new TemplateEntity
                {
                    Name = "Giftlist",
                    ViewPath = "Templates/Giftlist"
                }
            });

            var controller = new PublicPageController(fakeRepo, null);

            //Act
            var result = await controller.Display("greg", "my-gifts") as ViewResult;

            //Assert
            Assert.Equal("Templates/Giftlist", result?.ViewName);
        }

        [Fact]
        public async Task SaveInteraction()
        {
            //Arrange
            var fakeRepo = new FakePageRepository();
            var controller = new PublicPageController(fakeRepo, null);
            int testPageId = 123;
            string testKey = "item_checkbox_1";

            //Act
            await controller.ToggleInteraction(testPageId, testKey, "checked");

            //Assert
            var saved = await fakeRepo.GetInteractionAsync(testPageId, testKey);
            Assert.NotNull(saved);
            Assert.Equal("checked", saved.Value);
        }

        [Fact]
        public void SaveInteractionTime()
        {
            //Arrange Act
            var interaction = new PageInteraction { Value = "checked" };

            //Assert
            Assert.True(interaction.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }
    }
}
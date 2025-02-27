using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APIfilms.Models.EntityFramework;
using APIfilms.Controllers;
using APIfilms.Models.Repository;
using APIfilms.Models.DataManager;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace APIfilms.Tests
{
    [TestClass]
    public class UtilisateursControllerTests
    {
        private FilmRatingsDBContext context;
        private UtilisateursController controller;

        private IDataRepository<Utilisateur> dataRepository;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<FilmRatingsDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            context = new FilmRatingsDBContext(options);
            dataRepository = new UtilisateurManager(context);
            controller = new UtilisateursController(dataRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [TestMethod]
        public async Task GetAllUtilisateurs_ReturnsAllUsers()
        {
            context.Utilisateurs.AddRange(new List<Utilisateur>
            {
                new Utilisateur { Nom = "Alice", Mail = "alice@test.com" },
                new Utilisateur { Nom = "Bob", Mail = "bob@test.com" }
            });
            await context.SaveChangesAsync();

            var result = await controller.GetUtilisateurs();
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetUtilisateurById_ReturnsCorrectUser()
        {
            var user = new Utilisateur { Nom = "Test", Prenom = "User", Mail = "test@example.com" };
            context.Utilisateurs.Add(user);
            await context.SaveChangesAsync();

            var result = await controller.GetUtilisateurById(user.UtilisateurId);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(user.Mail, result.Value.Mail);
        }

        [TestMethod]
        public async Task GetUtilisateurById_ReturnsNotFound()
        {
            var result = await controller.GetUtilisateurById(999);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PostUtilisateur_CreatesNewUser()
        {
            var newUser = new Utilisateur
            {
                Nom = "Dupont",
                Prenom = "Jean",
                Mail = "dupont@test.com",
                Pwd = "Test1234!"
            };

            var result = await controller.PostUtilisateur(newUser);
            var createdUser = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Mail == newUser.Mail);

            Assert.IsNotNull(createdUser);
            Assert.AreEqual(newUser.Mail, createdUser.Mail);
        }

        [TestMethod]
        public async Task DeleteUtilisateur_RemovesUser()
        {
            var user = new Utilisateur { Nom = "Test", Prenom = "User", Mail = "delete@example.com" };
            context.Utilisateurs.Add(user);
            await context.SaveChangesAsync();

            var result = await controller.DeleteUtilisateur(user.UtilisateurId);
            var deletedUser = await context.Utilisateurs.FindAsync(user.UtilisateurId);

            Assert.IsNull(deletedUser);
        }

        [TestMethod]
        public void GetUtilisateurById_ExistingIdPassed_ReturnsRightItem_AvecMoq()
        {
            var user = new Utilisateur { UtilisateurId = 1, Nom = "Calida", Mail = "clilleymd@last.fm" };
            var mockRepository = new Mock<IDataRepository<Utilisateur>>();
            mockRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            var userController = new UtilisateursController(mockRepository.Object);
            var actionResult = userController.GetUtilisateurById(1).Result;

            Assert.IsNotNull(actionResult);
            Assert.IsNotNull(actionResult.Value);
            Assert.AreEqual(user.Mail, actionResult.Value.Mail);
        }
    }
}
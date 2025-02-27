using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using APIfilms.Models.EntityFramework;
using APIfilms.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using APIfilms.Models.Repository;
using TP4P1.Models.DataManager;
using Humanizer;
using Moq;

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

        [TestMethod]
        public async Task GetAllUtilisateurs_ReturnsAllUsers()
        {
            var result = await controller.GetUtilisateurs();
            Assert.AreEqual(context.Utilisateurs.Count(), result.Value.Count());
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
        public void GetUtilisateurById_ExistingIdPassed_ReturnsRightItem_AvecMoq()
        {
            // Arrange
            Utilisateur user = new Utilisateur
            {
                UtilisateurId = 1,
                Nom = "Calida",
                Prenom = "Lilley",
                Mobile = "0653930778",
                Mail = "clilleymd@last.fm",
                Pwd = "Toto12345678!",
                Rue = "Impasse des bergeronnettes",
                CodePostal = "74200",
                Ville = "Allinges",
                Pays = "France",
                Latitude = 46.344795F,
                Longitude = 6.4885845F
            };

            var mockRepository = new Mock<IDataRepository<Utilisateur>>();

            mockRepository.Setup(x => x.GetByIdAsync(1).Result).Returns(user);

            var userController = new UtilisateursController(mockRepository.Object);

            // Act
            var actionResult = userController.GetUtilisateurById(1).Result;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsNotNull(actionResult.Value);
            Assert.AreEqual(user, actionResult.Value as Utilisateur);
        }

        [TestMethod]
        public async Task PostUtilisateur_CreatesNewUser()
        {
            Random rnd = new Random();
            int uniqueId = rnd.Next(1, 1000000000);

            var newUser = new Utilisateur
            {
                Nom = "Dupont",
                Prenom = "Jean",
                Mail = $"dupont{uniqueId}@test.com",
                Pwd = "Test1234!"
            };

            var result = await controller.PostUtilisateur(newUser);
            var createdUser = await context.Utilisateurs.Where(u => u.Mail == newUser.Mail).FirstOrDefaultAsync();

            Assert.IsNotNull(createdUser);
            Assert.AreEqual(newUser.Mail, createdUser.Mail);
        }

        [TestMethod]
        public async Task PostUtilisateur_DuplicateEmail_ReturnsConflict()
        {
            var user = new Utilisateur { Nom = "Test", Prenom = "User", Mail = "duplicate@example.com" };
            context.Utilisateurs.Add(user);
            await context.SaveChangesAsync();

            var duplicateUser = new Utilisateur { Nom = "Test2", Prenom = "User2", Mail = "duplicate@example.com" };
            var result = await controller.PostUtilisateur(duplicateUser);

            Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
        }

        [TestMethod]
        public void Postutilisateur_ModelValidated_CreationOK_AvecMoq()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository<Utilisateur>>();
            var userController = new UtilisateursController(mockRepository.Object);

            Utilisateur user = new Utilisateur
            {
                Nom = "POISSON",
                Prenom = "Pascal",
                Mobile = "1",
                Mail = "poisson@gmail.com",
                Pwd = "Toto12345678!",
                Rue = "Chemin de Bellevue",
                CodePostal = "74940",
                Ville = "Annecy-le-Vieux",
                Pays = "France",
                Latitude = null,
                Longitude = null
            };

            // Act
            var actionResult = userController.PostUtilisateur(user).Result;

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ActionResult<Utilisateur>), "Pas un ActionResult<Utilisateur>");
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult), "Pas un CreatedAtActionResult");

            var result = actionResult.Result as CreatedAtActionResult;

            Assert.IsInstanceOfType(result.Value, typeof(Utilisateur), "Pas un Utilisateur");

            user.UtilisateurId = ((Utilisateur)result.Value).UtilisateurId;

            Assert.AreEqual(user, (Utilisateur)result.Value, "Utilisateurs pas identiques");
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
    }
}

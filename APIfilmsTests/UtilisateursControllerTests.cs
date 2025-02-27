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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIfilms.Controllers;
using APIfilms.Models.EntityFramework;
using APIfilms.Models.Repository;
using TP4P1.Models.DataManager;

namespace APIfilmsTests
{
    [TestClass]
    public class UtilisateursControllerTests
    {
        private UtilisateursController controller;
        private FilmRatingsDBContext context;
        private IDataRepository<Utilisateur> dataRepository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FilmRatingsDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            context = new FilmRatingsDBContext(options);
            dataRepository = new UtilisateurManager(context);
            controller = new UtilisateursController(dataRepository);

            context.Utilisateurs.Add(new Utilisateur { UtilisateurId = 1, Nom = "Durand", Prenom = "Pierre", Mail = "pierre@mail.com" });
            context.Utilisateurs.Add(new Utilisateur { UtilisateurId = 2, Nom = "Dupont", Prenom = "Marie", Mail = "marie@mail.com" });
            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetUtilisateurs_ReturnsAllUsers()
        {
            var result = await controller.GetUtilisateurs();
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task GetUtilisateurById_ReturnsUser_WhenUserExists()
        {
            var result = await controller.GetUtilisateurById(1);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Pierre", result.Value.Prenom);
        }

        [TestMethod]
        public async Task PostUtilisateur_AddsNewUser()
        {
            var newUser = new Utilisateur { UtilisateurId = 3, Nom = "Martin", Prenom = "Lucas", Mail = "lucas@mail.com" };
            var result = await controller.PostUtilisateur(newUser);
            Assert.AreEqual(3, context.Utilisateurs.Count());
        }
    }
}
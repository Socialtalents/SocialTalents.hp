using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.MongoDB;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    [TestClass]
    public class IdTests
    {
        public class Document: TypedIdMongoDocument<Document>, IEquatable<Document>
        {

            #region Equality members

            public bool Equals(Document other) => Id == other?.Id;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Document)obj);
            }

            public override int GetHashCode() => Id.GetHashCode();

            public static bool operator ==(Document left, Document right) => Equals(left, right);

            public static bool operator !=(Document left, Document right) => !Equals(left, right);

            #endregion

        }

        public class DocumentWithNullableReference: Document
        {
            public Id<Document>? Reference { get; set; }
        }

        private readonly IRepository<Document> _repository;

        public IdTests() => _repository = new MongoTypedIdRepository<Document>(new TestDatabase().MongoDatabase);

        [TestMethod]
        public void Id_Comparison()
        {
            const int skippedDocuments = 3;

            var documents = Enumerable.Repeat(0, 5).Select(_ => new Document()).ToArray();
            foreach (var document in documents) _repository.Insert(document);

            var documentsAfterSkipped = _repository.Where(d => d.Id > documents[skippedDocuments - 1].Id).ToList();

            CollectionAssert.AreEquivalent(
                expected: documents.Skip(skippedDocuments).ToList(),
                actual: documentsAfterSkipped);
        }

        [TestMethod]
        public void Id_Nullable()
        {
            var documents = new List<DocumentWithNullableReference>
            {
                new DocumentWithNullableReference(),
                new DocumentWithNullableReference { Reference = Id<Document>.GenerateNewId() }
            };

            foreach (var document in documents) _repository.Insert(document);
            var deserializedDocuments = _repository.ToList();

            CollectionAssert.AreEquivalent(
                expected: documents,
                actual: deserializedDocuments);
        }
    }
}
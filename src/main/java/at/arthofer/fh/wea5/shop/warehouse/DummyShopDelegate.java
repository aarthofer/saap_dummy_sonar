package at.arthofer.fh.wea5.shop.warehouse;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class DummyShopDelegate implements ShopDelegate {

	public Map<String, ArticleData> articles = new HashMap<String, ArticleData>();
	
	public DummyShopDelegate() {
		
		ArticleData art1 = new ArticleData("1", 
				"978-0201633610", 
				"Erich Gamma, Richard Helm, und Ralph E. Johnson", 
				"Design Patterns. Elements of Reusable Object-Oriented Software.",
				"Addison-Wesley Longman",
				"1995",
				"46.95");
		ArticleData art2 = new ArticleData("2",
				"978-3827415349",
				"Oliver Vogel, Ingo Arnold, Arif Chughtai, Markus Völter",
				"Software-Architektur. Grundlagen - Konzepte - Praxis",
				"Spektrum Akademischer Verlag",
				"2005",
				"49.95"
				);
		ArticleData art3 = new ArticleData("3",
				"978-3827370570",
				"Andrew S. Tanenbaum, Maarten van Steen",
				"Verteilte Systeme. Grundlagen und Paradigmen",
				"Pearson Studium",
				"2003",
				"49.95"
				);
		ArticleData art4 = new ArticleData("4",
				"978-0133065107",
				"Gregor Hohpe, Bobby Woolf",
				"Enterprise Integration Patterns: Designing, Building, and Deploying Messaging Solutions ",
				"Addision-Wesley",
				"2012",
				"26.25"
				);
		
		articles.put(art1.getId(), art1);
		articles.put(art2.getId(), art2);
		articles.put(art3.getId(), art3);
		articles.put(art4.getId(), art4);
	}
	
	public List<ArticleData> getAllArticles() {
		List<ArticleData> list = new ArrayList<ArticleData>();
		list.addAll(articles.values());
		return list;
	}

	public ArticleData getArticleById(String id) {
		return articles.get(id);
	}

}

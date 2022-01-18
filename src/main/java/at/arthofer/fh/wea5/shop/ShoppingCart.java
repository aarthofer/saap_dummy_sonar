package at.arthofer.fh.wea5.shop;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

import javax.enterprise.context.SessionScoped;
import javax.inject.Named;

import at.arthofer.fh.wea5.shop.warehouse.ArticleData;

@Named("shoppingCart")
@SessionScoped
public class ShoppingCart implements Serializable {

	private static final long serialVersionUID = 1L;

	private List<ArticleData> articles = new ArrayList<>();
	
	public List<ArticleData> getArticles() {
		return articles;
	}
	
	public void setArticles(List<ArticleData> articles) {
		this.articles = articles;
	}
	
	public void addArticle(ArticleData article) {
		if (article != null) {
			this.articles.add(article);
		}
	}
	
	public void removeArticle(String articleId) {
		this.articles.removeIf(a -> a.getId().equals(articleId));
	}
	
	public void clear() {
		this.articles = new ArrayList<>();
	}
	
	public float getShoppingCartSum() {
		float sum = 0f;
		for (ArticleData article : articles) {
			sum +=  Float.parseFloat(article.getPrice());
		}
		return sum;
	}
	
}

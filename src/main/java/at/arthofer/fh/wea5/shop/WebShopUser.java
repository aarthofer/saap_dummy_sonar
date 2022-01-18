package at.arthofer.fh.wea5.shop;

import java.util.ArrayList;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.enterprise.context.SessionScoped;
import javax.inject.Inject;
import javax.inject.Named;

import at.arthofer.fh.wea5.shop.util.Constants;
import at.arthofer.fh.wea5.shop.util.FacesUtil;
import at.arthofer.fh.wea5.shop.warehouse.ArticleData;
import at.arthofer.fh.wea5.shop.warehouse.ShopDelegate;
import java.io.Serializable;

@Named
@SessionScoped
public class WebShopUser implements Serializable {

	private static final long serialVersionUID = 1L;
	private static final Logger logger = Logger.getAnonymousLogger();
	
	private List<ArticleData> allArticles = new ArrayList<>();
	private ArticleData article;
	
	@Inject
	ShoppingCart shoppingCart;
	
	public ArticleData getArticle() {
		return article;
	}

	public List<ArticleData> getAllArticles() {
		return allArticles;
	}

	public String allArticles() {
		try {
			ShopDelegate shopDelegate = ServiceLocator.getInstance().getShopDelegate();
			this.allArticles = shopDelegate.getAllArticles();
			return Constants.SHOW_ALL_ARTICLES;
		} catch (Exception ex) {
			ex.printStackTrace();
			return null;
		}
	}
	
	public String articleDetails(String articleId) {
		try {
			logger.log(Level.INFO, "articleId :" + articleId);

			ShopDelegate shopDelegate = ServiceLocator.getInstance().getShopDelegate();
			this.article = shopDelegate.getArticleById(articleId);

			return Constants.SHOW_DETAILS;
		} catch (Exception e) {
			return FacesUtil.createFailure("Cannot get details for article!", e, logger);
		}
	}
	
	public String putArticleIntoCart(String articleId) {
		try {
			logger.log(Level.INFO, "articleId :" + articleId);

			ShopDelegate shopDelegate = ServiceLocator.getInstance().getShopDelegate();
			ArticleData article = shopDelegate.getArticleById(articleId);

			shoppingCart.addArticle(article);

			return Constants.SHOW_CART;
		} catch (Exception e) {
			return FacesUtil.createFailure("Cannot put article into shopping cart!", e, logger);
		}
	}

}

package at.arthofer.fh.wea5.shop.warehouse;

import java.util.List;

public interface ShopDelegate {

	public abstract List<ArticleData> getAllArticles();

	public abstract ArticleData getArticleById(String id);

}
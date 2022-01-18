package at.arthofer.fh.wea5.shop.warehouse;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import at.arthofer.fh.wea5.shop.ServiceLocator;

/*
 * This delegate provides test data from the database.
 */
public class DBShopDelegate implements ShopDelegate {

	private static Logger logger = Logger.getLogger("WEA5Bookshop");

	/*
	 * Get all articles.
	 */
	public List<ArticleData> getAllArticles() {
		Connection connection = null;
		try {
			connection = ServiceLocator.getInstance().getDBConnection();
		} catch (Exception e) {
			logger.log(Level.SEVERE, e.toString());
		}

		List<ArticleData> articles = new ArrayList<ArticleData>();
		try {

			// Get data from DB.
			Statement statement = connection.createStatement();
			ResultSet resultSet = statement.executeQuery("SELECT * FROM Books");
			while (resultSet.next()) {
				ArticleData a = new ArticleData(resultSet.getString("ID"), resultSet.getString("ISBN"),
						resultSet.getString("Author"), resultSet.getString("Title"), resultSet.getString("Publisher"),
						resultSet.getString("PubYear"), resultSet.getString("Price"));
				articles.add(a);
			}
		} catch (SQLException e) {
			logger.log(Level.SEVERE, e.toString());
		} finally {
			try {
				connection.close();
			} catch (Exception e) {
				// Ignore.
			}
		}
		return articles;
	}

	/*
	 * Get specific article by id.
	 */
	public ArticleData getArticleById(String id) {
		Connection connection = null;
		ArticleData article = null;
		try {
			connection = ServiceLocator.getInstance().getDBConnection();
		} catch (Exception e) {
			logger.log(Level.SEVERE, e.toString());
		}

		try {
			logger.log(Level.INFO, "id : " + id);
			// get data from DB
			
			// unsecure
			// Statement statement = connection.createStatement();
			// ResultSet resultSet = statement.executeQuery("SELECT * FROM Books WHERE ID=" + id);
			
			// secure
			String selectStatement = "SELECT * FROM Books WHERE ID = ?";
			PreparedStatement statement = connection.prepareStatement(selectStatement);
			statement.setString(1, id);
			ResultSet resultSet = statement.executeQuery();
			if (resultSet.next()) {
				article = new ArticleData(resultSet.getString("ID"), resultSet.getString("ISBN"),
						resultSet.getString("Author"), resultSet.getString("Title"), resultSet.getString("Publisher"),
						resultSet.getString("PubYear"), resultSet.getString("Price"));
			}
		} catch (SQLException e) {
			logger.log(Level.SEVERE, e.toString());
		} finally {
			try {
				connection.close();
			} catch (Exception e) {
				// Ignore.
			}
		}
		return article;
	}

}

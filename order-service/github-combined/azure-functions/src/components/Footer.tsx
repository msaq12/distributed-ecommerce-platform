export default function Footer() {
  return (
    <footer className="bg-gray-900 text-gray-300 mt-auto">
      <div className="max-w-7xl mx-auto px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Brand */}
          <div>
            <h3 className="text-xl font-bold text-white mb-4">
              Furniture<span className="text-blue-400">Hub</span>
            </h3>
            <p className="text-sm text-gray-400">
              Premium furniture for modern living. Quality craftsmanship
              delivered to your door.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="text-sm font-semibold text-white uppercase tracking-wider mb-4">
              Quick Links
            </h4>
            <ul className="space-y-2 text-sm">
              <li>
                <a
                  href="/products"
                  className="hover:text-white transition-colors"
                >
                  All Products
                </a>
              </li>
              <li>
                <a
                  href="/products?category=sofas"
                  className="hover:text-white transition-colors"
                >
                  Sofas
                </a>
              </li>
              <li>
                <a
                  href="/products?category=chairs"
                  className="hover:text-white transition-colors"
                >
                  Chairs
                </a>
              </li>
              <li>
                <a
                  href="/products?category=tables"
                  className="hover:text-white transition-colors"
                >
                  Tables
                </a>
              </li>
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h4 className="text-sm font-semibold text-white uppercase tracking-wider mb-4">
              Customer Support
            </h4>
            <ul className="space-y-2 text-sm">
              <li>Email: support@furniturehub.com</li>
              <li>Phone: 1-800-FURNITURE</li>
              <li>Hours: Mon-Fri 9am-5pm EST</li>
            </ul>
          </div>
        </div>

        <div className="mt-8 pt-8 border-t border-gray-800 text-center text-sm text-gray-500">
          <p>
            &copy; {new Date().getFullYear()} FurnitureHub. Azure Dropshipping
            Platform Demo.
          </p>
        </div>
      </div>
    </footer>
  );
}
